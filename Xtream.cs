using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace IPTVInfo
{
    public class XtreamIPTV
    {
        public XtreamIPTV()
        {
        }

        public XtreamIPTV(string serverUrl, string username, string password)
        {
            ServerUrl = serverUrl;
            Username = username;
            Password = password;
        }

        public XtreamIPTV(string m3uUrl)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(m3uUrl);

            if (!Uri.TryCreate(m3uUrl, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("The M3U URL must be an absolute HTTP or HTTPS URL.", nameof(m3uUrl));
            }

            string? username = null;
            string? password = null;
            foreach (string parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = parameter.Split('=', 2);
                string key = Uri.UnescapeDataString(parts[0].Replace('+', ' '));
                string value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1].Replace('+', ' ')) : string.Empty;

                if (key.Equals("username", StringComparison.OrdinalIgnoreCase))
                {
                    username = value;
                }
                else if (key.Equals("password", StringComparison.OrdinalIgnoreCase))
                {
                    password = value;
                }
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("The M3U URL must include username and password query parameters.", nameof(m3uUrl));
            }

            string host = uri.HostNameType == UriHostNameType.IPv6 ? $"[{uri.Host}]" : uri.Host;
            ServerUrl = $"{uri.Scheme}://{host}:{uri.Port}";
            Username = username;
            Password = password;
        }

        public string ServerUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public Task<int> GetTvChannelCountAsync() => GetCategoryCountAsync("get_live_categories");

        public Task<int> GetMovieCountAsync() => GetCategoryCountAsync("get_vod_categories");

        public Task<int> GetTvShowCountAsync() => GetCategoryCountAsync("get_series_categories");

        public async Task<(bool Success, int TestedCount, List<string> Diagnostics)> TestStreamAsync()
        {
            List<string> diagnostics = [];

            UriBuilder channelsUri = new($"{ServerUrl.TrimEnd('/')}/player_api.php")
            {
                Query = BuildCredentialsQuery("get_live_streams")
            };

            List<LiveChannel> sampledChannels;
            using CancellationTokenSource channelListTimeout = new(TimeSpan.FromMinutes(5));
            using (HttpClient channelsClient = new() { Timeout = Timeout.InfiniteTimeSpan })
            using (HttpResponseMessage channelsResponse = await channelsClient.GetAsync(
                channelsUri.Uri,
                HttpCompletionOption.ResponseHeadersRead,
                channelListTimeout.Token))
            {
                channelsResponse.EnsureSuccessStatusCode();
                await using Stream channelStream = await channelsResponse.Content.ReadAsStreamAsync(channelListTimeout.Token);
                sampledChannels = await SelectRandomChannelsAsync(channelStream, channelListTimeout.Token);
            }

            if (sampledChannels.Count == 0)
            {
                diagnostics.Add("No live channels were returned by the provider for stream testing.");
                return (false, 0, diagnostics);
            }

            bool anyStreamWorks = false;
            using HttpClient streamClient = new() { Timeout = TimeSpan.FromSeconds(8) };
            for (int index = 0; index < sampledChannels.Count; index++)
            {
                LiveChannel channel = sampledChannels[index];
                string streamId = GetStreamId(channel.StreamId)!;
                string streamPath = string.Join('/', Uri.EscapeDataString(Username), Uri.EscapeDataString(Password), Uri.EscapeDataString(streamId));
                Uri streamUri = new(new Uri(ServerUrl.TrimEnd('/') + "/"), streamPath);
                string channelUrl = $"{ServerUrl.TrimEnd('/')}/{streamPath}";
                string channelLabel = $"Channel {index + 1}/{sampledChannels.Count}, stream {streamId} ({channelUrl})";
                System.Net.HttpStatusCode? receivedStatusCode = null;

                try
                {
                    using HttpRequestMessage request = new(HttpMethod.Get, streamUri);
                    request.Headers.Range = new RangeHeaderValue(0, 511);
                    request.Headers.ConnectionClose = true;
                    using HttpResponseMessage response = await streamClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    receivedStatusCode = response.StatusCode;

                    MediaTypeHeaderValue? contentType = response.Content?.Headers.ContentType;
                    string statusText = contentType is not null
                        ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}; Content-Type: {contentType}"
                        : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        diagnostics.Add($"{channelLabel} -> {statusText}; stopping the test after the first HTTP 200 response.");
                        return (true, index + 1, diagnostics);
                    }

                    if (!response.IsSuccessStatusCode || IsErrorContentType(contentType))
                    {
                        string? responseBody = response.Content is null ? null : await ReadResponseTextAsync(response.Content);
                        string detail = responseBody is null
                            ? statusText
                            : $"{statusText}; Server response: {Truncate(responseBody, 200)}";
                        diagnostics.Add($"{channelLabel} -> {detail}");
                        continue;
                    }

                    await using Stream stream = await response.Content!.ReadAsStreamAsync();
                    byte[] sample = new byte[512];
                    int bytesRead = await stream.ReadAsync(sample.AsMemory(0, sample.Length), CancellationToken.None);
                    if (bytesRead > 0 && !LooksLikeErrorPayload(sample.AsSpan(0, bytesRead)))
                    {
                        diagnostics.Add($"{channelLabel} -> {statusText}; valid media sample received ({bytesRead} bytes).");
                        anyStreamWorks = true;
                        continue;
                    }

                    string payloadText = bytesRead > 0
                        ? $" Server response: {Truncate(Encoding.UTF8.GetString(sample, 0, bytesRead), 200)}"
                        : string.Empty;
                    diagnostics.Add($"{channelLabel} -> {statusText}; response payload looked like an error or empty stream.{payloadText}");
                }
                catch (HttpRequestException ex)
                {
                    System.Net.HttpStatusCode? statusCode = receivedStatusCode ?? ex.StatusCode;
                    string status = statusCode is { } code
                        ? $"HTTP {(int)code} {code}; "
                        : "No HTTP response; ";
                    diagnostics.Add($"{channelLabel} -> {status}HttpRequestException: {BuildExceptionDetails(ex)}");
                }
                catch (TaskCanceledException ex)
                {
                    string status = receivedStatusCode is { } code
                        ? $"HTTP {(int)code} {code}; "
                        : "No HTTP response; ";
                    diagnostics.Add($"{channelLabel} -> {status}TaskCanceledException: {BuildExceptionDetails(ex)}");
                }
                catch (IOException ex)
                {
                    string status = receivedStatusCode is { } code
                        ? $"HTTP {(int)code} {code}; "
                        : "No HTTP response; ";
                    diagnostics.Add($"{channelLabel} -> {status}IOException: {BuildExceptionDetails(ex)}");
                }
            }

            return (anyStreamWorks, sampledChannels.Count, diagnostics);
        }

        public async Task<int> GetLiveChannelCountAsync()
        {
            UriBuilder channelsUri = new($"{ServerUrl.TrimEnd('/')}/player_api.php")
            {
                Query = BuildCredentialsQuery("get_live_streams")
            };

            using HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(channelsUri.Uri);
            response.EnsureSuccessStatusCode();

            await using Stream channelStream = await response.Content.ReadAsStreamAsync();
            List<LiveChannel?>? channels = await JsonSerializer.DeserializeAsync<List<LiveChannel?>>(channelStream);
            return (channels ?? []).Count(channel =>
                channel is not null && !string.IsNullOrWhiteSpace(GetStreamId(channel.StreamId)));
        }

        private static async Task<string?> ReadResponseTextAsync(HttpContent? content)
        {
            if (content is null)
            {
                return null;
            }

            try
            {
                if (content.Headers.ContentLength > 1_048_576)
                {
                    return null;
                }

                string body = await content.ReadAsStringAsync();
                return string.IsNullOrWhiteSpace(body) ? null : body.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static string BuildExceptionDetails(Exception ex)
        {
            List<string> details = [];
            for (Exception? current = ex; current is not null; current = current.InnerException)
            {
                details.Add($"{current.GetType().Name}: {current.Message}");
            }

            return string.Join(" Inner: ", details);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            string normalized = value.Replace("\r", " ").Replace("\n", " ").Trim();
            return normalized.Length <= maxLength
                ? normalized
                : $"{normalized[..(maxLength - 3)]}...";
        }

        private static async Task<List<LiveChannel>> SelectRandomChannelsAsync(Stream channelStream, CancellationToken cancellationToken)
        {
            List<LiveChannel?>? channelList = await JsonSerializer.DeserializeAsync<List<LiveChannel?>>(
                channelStream,
                cancellationToken: cancellationToken);

            List<LiveChannel> validChannels = new(channelList?.Count ?? 0);
            foreach (LiveChannel? channel in channelList ?? [])
            {
                if (channel is not null && !string.IsNullOrWhiteSpace(GetStreamId(channel.StreamId)))
                {
                    validChannels.Add(channel);
                }
            }

            int sampleCount = Math.Min(10, validChannels.Count);
            for (int index = 0; index < sampleCount; index++)
            {
                int randomIndex = Random.Shared.Next(index, validChannels.Count);
                (validChannels[index], validChannels[randomIndex]) = (validChannels[randomIndex], validChannels[index]);
            }

            if (sampleCount < validChannels.Count)
            {
                validChannels.RemoveRange(sampleCount, validChannels.Count - sampleCount);
            }

            return validChannels;
        }

        private static string? GetStreamId(JsonElement streamId)
        {
            return streamId.ValueKind switch
            {
                JsonValueKind.String => streamId.GetString(),
                JsonValueKind.Number => streamId.GetRawText(),
                _ => null
            };
        }

        private static bool IsErrorContentType(MediaTypeHeaderValue? contentType)
        {
            string? mediaType = contentType?.MediaType;
            return mediaType is not null &&
                (mediaType.Contains("html", StringComparison.OrdinalIgnoreCase) ||
                 mediaType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
                 mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase) ||
                 mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase));
        }

        private static bool LooksLikeErrorPayload(ReadOnlySpan<byte> sample)
        {
            string prefix = Encoding.UTF8.GetString(sample[..Math.Min(sample.Length, 256)])
                .TrimStart('\uFEFF', ' ', '\t', '\r', '\n');
            return prefix.StartsWith("<", StringComparison.Ordinal) ||
                prefix.StartsWith("{", StringComparison.Ordinal) ||
                prefix.StartsWith("[", StringComparison.Ordinal) ||
                prefix.StartsWith("error", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("unauthorized", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("forbidden", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("not found", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("expired", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("invalid", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("denied", StringComparison.OrdinalIgnoreCase);
        }

        private string BuildCredentialsQuery(string? action = null)
        {
            string query = $"username={Uri.EscapeDataString(Username)}&password={Uri.EscapeDataString(Password)}";
            return action is null ? query : $"{query}&action={Uri.EscapeDataString(action)}";
        }

        public async Task<XtreamAccount> GetServerDetailsAsync()
        {
            UriBuilder apiUri = new($"{ServerUrl.TrimEnd('/')}/player_api.php")
            {
                Query = $"username={Uri.EscapeDataString(Username)}&password={Uri.EscapeDataString(Password)}"
            };

            using HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(apiUri.Uri);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            JObject payload = JObject.Parse(jsonResponse);
            JObject userInfo = payload["user_info"] as JObject
                ?? throw new Newtonsoft.Json.JsonSerializationException("The Xtream response did not contain user_info.");
            JObject? serverInfo = payload["server_info"] as JObject;

            return new XtreamAccount
            {
                Status = userInfo.Value<string>("status"),
                ExpirationDate = ParseExpirationDate(userInfo["exp_date"]),
                IsTrial = ParseBoolean(userInfo["is_trial"]),
                ActiveConnections = ParseInteger(userInfo["active_cons"]),
                MaxConnections = ParseInteger(userInfo["max_connections"]),
                Server = serverInfo is null ? null : new XtreamServer
                {
                    Url = serverInfo.Value<string>("url"),
                    Port = serverInfo.Value<string>("port"),
                    HttpsPort = serverInfo.Value<string>("https_port"),
                    Protocol = serverInfo.Value<string>("server_protocol"),
                    Timezone = serverInfo.Value<string>("timezone"),
                    TimeNow = serverInfo.Value<string>("time_now")
                }
            };
        }

        private static DateTimeOffset? ParseExpirationDate(JToken? value)
        {
            return long.TryParse(value?.ToString(), out long unixSeconds) && unixSeconds > 0
                ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
                : null;
        }

        private static int? ParseInteger(JToken? value)
        {
            return int.TryParse(value?.ToString(), out int result) ? result : null;
        }

        private static bool? ParseBoolean(JToken? value)
        {
            string? text = value?.ToString();
            if (bool.TryParse(text, out bool result)) return result;
            if (int.TryParse(text, out int numericValue)) return numericValue != 0;
            return null;
        }

        private async Task<int> GetCategoryCountAsync(string action)
        {
            try
            {
                using HttpClient client = new();
                string apiUrl = $"{ServerUrl.TrimEnd('/')}/player_api.php?username={Uri.EscapeDataString(Username)}&password={Uri.EscapeDataString(Password)}&action={Uri.EscapeDataString(action)}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return 0;

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JArray data = JArray.Parse(jsonResponse);
                return data.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }

        public async Task<(int tvChannels, int movies, int tvShows)> GetXtreamStatsAsync()
        {
            string serverUrl = ServerUrl;
            string username = Username;
            string password = Password;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    async Task<int> GetCountAsync(string category)
                    {
                        string apiUrl = $"{serverUrl}/player_api.php?username={username}&password={password}&action=get_{category}";
                        HttpResponseMessage response = await client.GetAsync(apiUrl);
                        if (!response.IsSuccessStatusCode) return 0;
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JArray data = JArray.Parse(jsonResponse);
                        return data.Count;
                    }

                    int tvChannels = await GetTvChannelCountAsync();
                    int movies = await GetMovieCountAsync();
                    int tvShows = await GetTvShowCountAsync();

                    return (tvChannels, movies, tvShows);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (0, 0, 0);
            }
        }
    }

    internal sealed class LiveChannel
    {
        [JsonPropertyName("stream_id")]
        public JsonElement StreamId { get; set; }

        [JsonPropertyName("container_extension")]
        public string? ContainerExtension { get; set; }
    }

    public class XtreamAccount
    {
        public string? Status { get; init; }
        public DateTimeOffset? ExpirationDate { get; init; }
        public bool? IsTrial { get; init; }
        public int? ActiveConnections { get; init; }
        public int? MaxConnections { get; init; }
        public XtreamServer? Server { get; init; }
    }

    public class XtreamServer
    {
        public string? Url { get; init; }
        public string? Port { get; init; }
        public string? HttpsPort { get; init; }
        public string? Protocol { get; init; }
        public string? Timezone { get; init; }
        public string? TimeNow { get; init; }
    }

}
