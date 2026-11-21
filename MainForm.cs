using System.Globalization;
using System.Text.RegularExpressions;
using IPTVInfo;
public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        _fullUrlMode.CheckedChanged += (_, _) => UpdateInputMode();
        _loadButton.Click += async (_, _) => await LoadDetailsAsync();
        _testStreamButton.Click += async (_, _) => await TestStreamAsync();
        _saveCsvButton.Click += (_, _) => SaveGridToCsv();
        UpdateInputMode();
    }

    private void UpdateInputMode()
    {
        bool useFullUrl = _fullUrlMode.Checked;
        _fullUrlInput.Enabled = useFullUrl;
        _serverInput.Enabled = !useFullUrl;
        _usernameInput.Enabled = !useFullUrl;
        _passwordInput.Enabled = !useFullUrl;
    }

    private XtreamIPTV CreateClient()
    {
        return _fullUrlMode.Checked
            ? new XtreamIPTV(_fullUrlInput.Text.Trim())
            : new XtreamIPTV(_serverInput.Text.Trim(), _usernameInput.Text.Trim(), _passwordInput.Text);
    }

    private async Task TestStreamAsync()
    {
        _loadButton.Enabled = false;
        _testStreamButton.Enabled = false;
        _status.Text = "Testing up to 10 random live channels and 10 random movies...";
        _details.Clear();

        try
        {
            var (streamWorks, testedCount, passedBy, diagnostics) = await CreateClient().TestStreamAsync();
            _details.Text = string.Join(Environment.NewLine, diagnostics);
            _details.AppendText(Environment.NewLine + Environment.NewLine +
                $"Final result: {(streamWorks ? $"Passed by {passedBy.ToLowerInvariant()}" : "Failed")}. " +
                $"Tested {testedCount} stream(s); " +
                (streamWorks ? "at least one valid stream was received." : "no valid stream was received."));
            _status.Text = $"Stream test {(streamWorks ? $"passed by {passedBy.ToLowerInvariant()}" : "failed")}; " +
                $"{testedCount} stream(s) tested. See results below.";
        }
        catch (Exception ex)
        {
            _status.Text = $"Could not test streams: {ex.Message}";
            _details.Text = $"Stream test could not complete: {ex}";
        }
        finally
        {
            _loadButton.Enabled = true;
            _testStreamButton.Enabled = true;
        }
    }

    private async Task TestUrlsFromFileAsync()
    {
        using OpenFileDialog dialog = new()
        {
            CheckFileExists = true,
            Filter = "Text files (*.txt;*.m3u;*.m3u8)|*.txt;*.m3u;*.m3u8|All files (*.*)|*.*",
            Title = "Select a file containing M3U URLs"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _loadButton.Enabled = false;
        _testStreamButton.Enabled = false;
        _importFileButton.Enabled = false;
        _accountsGrid.Rows.Clear();
        _details.Clear();

        try
        {
            string contents = await File.ReadAllTextAsync(dialog.FileName);
            List<string> urls = Regex.Matches(contents, @"https?://[^\s""'<>]+", RegexOptions.IgnoreCase)
                .Select(match => match.Value.TrimEnd('.', ',', ';', ':', ')', ']', '}'))
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (urls.Count == 0)
            {
                _status.Text = "No HTTP or HTTPS M3U URLs were found in the selected file.";
                return;
            }

            int urlsWithPassingTests = 0;
            int passingCategoryCount = 0;
            for (int index = 0; index < urls.Count; index++)
            {
                _status.Text = $"Testing URL {index + 1} of {urls.Count}...";

                try
                {
                    int passingCategories = await TestUrlAsync(urls[index], index + 1);
                    if (passingCategories > 0)
                    {
                        urlsWithPassingTests++;
                        passingCategoryCount += passingCategories;
                    }
                }
                catch (Exception ex)
                {
                    _details.AppendText($"URL {index + 1}: {ex.Message}{Environment.NewLine}");
                }
            }

            _status.Text = $"Finished testing {urls.Count} URL(s); {urlsWithPassingTests} passed, adding {passingCategoryCount} channel/movie result(s).";
            _resultsTabs.SelectedTab = _accountsTab;
        }
        catch (Exception ex)
        {
            _status.Text = $"Could not process the selected file: {ex.Message}";
        }
        finally
        {
            _loadButton.Enabled = true;
            _testStreamButton.Enabled = true;
            _importFileButton.Enabled = true;
        }
    }

    private async Task<int> TestUrlAsync(string url, int urlNumber)
    {
        XtreamIPTV client = new(url);
        var channelTest = await client.TestStreamCategoryAsync(movies: false);
        var movieTest = await client.TestStreamCategoryAsync(movies: true);

        AppendCategoryTestDetails(urlNumber, "TV channels", channelTest);
        AppendCategoryTestDetails(urlNumber, "Movies", movieTest);

        int passingCategories = (channelTest.Success ? 1 : 0) + (movieTest.Success ? 1 : 0);
        if (passingCategories == 0)
        {
            return 0;
        }

        string channelCount = "Unavailable";
        try
        {
            channelCount = (await client.GetLiveChannelCountAsync()).ToString(CultureInfo.CurrentCulture);
        }
        catch (Exception ex)
        {
            _details.AppendText($"URL {urlNumber}: channel count could not be loaded: {ex.Message}{Environment.NewLine}");
        }

        if (channelTest.Success)
        {
            _accountsGrid.Rows.Add(client.ServerUrl, client.Username, client.Password, channelCount, "TV channels");
        }

        if (movieTest.Success)
        {
            _accountsGrid.Rows.Add(client.ServerUrl, client.Username, client.Password, channelCount, "Movies");
        }

        return passingCategories;
    }

    private void AppendCategoryTestDetails(
        int urlNumber,
        string category,
        (bool Success, int TestedCount, List<string> Diagnostics) result)
    {
        _details.AppendText(
            $"URL {urlNumber}: {category} test {(result.Success ? "passed" : "failed")}; " +
            $"tested {result.TestedCount} stream(s).{Environment.NewLine}");

        foreach (string diagnostic in result.Diagnostics)
        {
            _details.AppendText($"  {diagnostic}{Environment.NewLine}");
        }
    }

    private void SaveGridToCsv()
    {
        if (_accountsGrid.Rows.Count == 0)
        {
            _status.Text = "There are no accounts to save.";
            return;
        }

        using SaveFileDialog dialog = new()
        {
            AddExtension = true,
            DefaultExt = "csv",
            FileName = "working-accounts.csv",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Save working accounts"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            using StreamWriter writer = new(dialog.FileName, false, new System.Text.UTF8Encoding(true));
            writer.WriteLine(string.Join(",", _accountsGrid.Columns
                .Cast<DataGridViewColumn>()
                .Select(column => EscapeCsvField(column.HeaderText))));

            foreach (DataGridViewRow row in _accountsGrid.Rows)
            {
                writer.WriteLine(string.Join(",", row.Cells
                    .Cast<DataGridViewCell>()
                    .Select(cell => EscapeCsvField(cell.FormattedValue?.ToString() ?? string.Empty))));
            }

            _status.Text = $"Saved {_accountsGrid.Rows.Count} account(s) to CSV.";
        }
        catch (Exception ex)
        {
            _status.Text = $"Could not save CSV: {ex.Message}";
        }
    }

    private static string EscapeCsvField(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private async Task LoadDetailsAsync()
    {
        _loadButton.Enabled = false;
        _status.Text = "Loading...";
        _details.Clear();

        try
        {
            XtreamAccount account = await CreateClient().GetServerDetailsAsync();
            _details.Text = string.Join(Environment.NewLine,
            [
                $"Status: {account.Status ?? "Not provided"}",
                $"Subscription expires: {account.ExpirationDate?.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) ?? "No expiry / not provided"}",
                $"Trial account: {account.IsTrial?.ToString() ?? "Not provided"}",
                $"Active connections: {account.ActiveConnections?.ToString(CultureInfo.CurrentCulture) ?? "Not provided"}",
                $"Maximum connections: {account.MaxConnections?.ToString(CultureInfo.CurrentCulture) ?? "Not provided"}",
                $"Server: {account.Server?.Url ?? "Not provided"}",
                $"Port: {account.Server?.Port ?? "Not provided"}",
                $"HTTPS port: {account.Server?.HttpsPort ?? "Not provided"}",
                $"Protocol: {account.Server?.Protocol ?? "Not provided"}",
                $"Timezone: {account.Server?.Timezone ?? "Not provided"}",
                $"Server time: {account.Server?.TimeNow ?? "Not provided"}"
            ]);
            _status.Text = "Details loaded.";
        }
        catch (Exception ex)
        {
            _status.Text = $"Could not load details: {ex.Message}";
        }
        finally
        {
            _loadButton.Enabled = true;
        }
    }

    private async void _importFileButton_Click(object sender, EventArgs e)
    {
        await TestUrlsFromFileAsync();
    }
}
