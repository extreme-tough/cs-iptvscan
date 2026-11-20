partial class MainForm
{
    private RadioButton _fullUrlMode = null!;
    private RadioButton _separateFieldsMode = null!;
    private TextBox _fullUrlInput = null!;
    private TextBox _serverInput = null!;
    private TextBox _usernameInput = null!;
    private TextBox _passwordInput = null!;
    private Button _loadButton = null!;
    private Button _testStreamButton = null!;
    private Label _status = null!;
    private TextBox _details = null!;
    private GroupBox _modeGroup = null!;
    private Label _fullUrlLabel = null!;
    private Label _serverLabel = null!;
    private Label _usernameLabel = null!;
    private Label _passwordLabel = null!;
    private Button _importFileButton = null!;
    private Button _saveCsvButton = null!;
    private TabControl _resultsTabs = null!;
    private TabPage _accountsTab = null!;
    private TabPage _detailsTab = null!;
    private DataGridView _accountsGrid = null!;

    private void InitializeComponent()
    {
        _modeGroup = new GroupBox();
        _fullUrlMode = new RadioButton();
        _separateFieldsMode = new RadioButton();
        _fullUrlInput = new TextBox();
        _serverInput = new TextBox();
        _usernameInput = new TextBox();
        _passwordInput = new TextBox();
        _loadButton = new Button();
        _testStreamButton = new Button();
        _importFileButton = new Button();
        _saveCsvButton = new Button();
        _status = new Label();
        _details = new TextBox();
        _resultsTabs = new TabControl();
        _accountsTab = new TabPage();
        _accountsGrid = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        _detailsTab = new TabPage();
        _fullUrlLabel = new Label();
        _serverLabel = new Label();
        _usernameLabel = new Label();
        _passwordLabel = new Label();
        _modeGroup.SuspendLayout();
        _resultsTabs.SuspendLayout();
        _accountsTab.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_accountsGrid).BeginInit();
        _detailsTab.SuspendLayout();
        SuspendLayout();
        // 
        // _modeGroup
        // 
        _modeGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _modeGroup.Controls.Add(_fullUrlMode);
        _modeGroup.Controls.Add(_separateFieldsMode);
        _modeGroup.Location = new Point(18, 21);
        _modeGroup.Margin = new Padding(3, 4, 3, 4);
        _modeGroup.Name = "_modeGroup";
        _modeGroup.Padding = new Padding(3, 4, 3, 4);
        _modeGroup.Size = new Size(722, 85);
        _modeGroup.TabIndex = 0;
        _modeGroup.TabStop = false;
        _modeGroup.Text = "Connection details";
        // 
        // _fullUrlMode
        // 
        _fullUrlMode.AutoSize = true;
        _fullUrlMode.Checked = true;
        _fullUrlMode.Location = new Point(18, 35);
        _fullUrlMode.Margin = new Padding(3, 4, 3, 4);
        _fullUrlMode.Name = "_fullUrlMode";
        _fullUrlMode.Size = new Size(118, 24);
        _fullUrlMode.TabIndex = 0;
        _fullUrlMode.TabStop = true;
        _fullUrlMode.Text = "Full M3U URL";
        // 
        // _separateFieldsMode
        // 
        _separateFieldsMode.AutoSize = true;
        _separateFieldsMode.Location = new Point(183, 35);
        _separateFieldsMode.Margin = new Padding(3, 4, 3, 4);
        _separateFieldsMode.Name = "_separateFieldsMode";
        _separateFieldsMode.Size = new Size(176, 24);
        _separateFieldsMode.TabIndex = 1;
        _separateFieldsMode.Text = "Server and credentials";
        // 
        // _fullUrlInput
        // 
        _fullUrlInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _fullUrlInput.Location = new Point(171, 125);
        _fullUrlInput.Margin = new Padding(3, 4, 3, 4);
        _fullUrlInput.Name = "_fullUrlInput";
        _fullUrlInput.Size = new Size(569, 27);
        _fullUrlInput.TabIndex = 5;
        // 
        // _serverInput
        // 
        _serverInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _serverInput.Location = new Point(171, 173);
        _serverInput.Margin = new Padding(3, 4, 3, 4);
        _serverInput.Name = "_serverInput";
        _serverInput.PlaceholderText = "https://server.example:8080";
        _serverInput.Size = new Size(569, 27);
        _serverInput.TabIndex = 6;
        // 
        // _usernameInput
        // 
        _usernameInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _usernameInput.Location = new Point(171, 221);
        _usernameInput.Margin = new Padding(3, 4, 3, 4);
        _usernameInput.Name = "_usernameInput";
        _usernameInput.Size = new Size(569, 27);
        _usernameInput.TabIndex = 7;
        // 
        // _passwordInput
        // 
        _passwordInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _passwordInput.Location = new Point(171, 269);
        _passwordInput.Margin = new Padding(3, 4, 3, 4);
        _passwordInput.Name = "_passwordInput";
        _passwordInput.Size = new Size(569, 27);
        _passwordInput.TabIndex = 8;
        _passwordInput.UseSystemPasswordChar = true;
        // 
        // _loadButton
        // 
        _loadButton.Location = new Point(18, 325);
        _loadButton.Margin = new Padding(3, 4, 3, 4);
        _loadButton.Name = "_loadButton";
        _loadButton.Size = new Size(128, 40);
        _loadButton.TabIndex = 9;
        _loadButton.Text = "Get details";
        // 
        // _testStreamButton
        // 
        _testStreamButton.Location = new Point(155, 325);
        _testStreamButton.Margin = new Padding(3, 4, 3, 4);
        _testStreamButton.Name = "_testStreamButton";
        _testStreamButton.Size = new Size(128, 40);
        _testStreamButton.TabIndex = 10;
        _testStreamButton.Text = "Test stream";
        // 
        // _importFileButton
        // 
        _importFileButton.Location = new Point(293, 325);
        _importFileButton.Margin = new Padding(3, 4, 3, 4);
        _importFileButton.Name = "_importFileButton";
        _importFileButton.Size = new Size(155, 40);
        _importFileButton.TabIndex = 11;
        _importFileButton.Text = "Test URLs from file";
        // 
        // _saveCsvButton
        // 
        _saveCsvButton.Location = new Point(459, 325);
        _saveCsvButton.Margin = new Padding(3, 4, 3, 4);
        _saveCsvButton.Name = "_saveCsvButton";
        _saveCsvButton.Size = new Size(105, 40);
        _saveCsvButton.TabIndex = 12;
        _saveCsvButton.Text = "Save CSV";
        // 
        // _status
        // 
        _status.AutoSize = true;
        _status.Location = new Point(36, 383);
        _status.MaximumSize = new Size(167, 53);
        _status.Name = "_status";
        _status.Size = new Size(0, 20);
        _status.TabIndex = 13;
        // 
        // _details
        // 
        _details.Dock = DockStyle.Fill;
        _details.Location = new Point(0, 0);
        _details.Margin = new Padding(3, 4, 3, 4);
        _details.Multiline = true;
        _details.Name = "_details";
        _details.ReadOnly = true;
        _details.ScrollBars = ScrollBars.Vertical;
        _details.Size = new Size(714, 251);
        _details.TabIndex = 0;
        // 
        // _resultsTabs
        // 
        _resultsTabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _resultsTabs.Controls.Add(_accountsTab);
        _resultsTabs.Controls.Add(_detailsTab);
        _resultsTabs.Location = new Point(18, 417);
        _resultsTabs.Margin = new Padding(3, 4, 3, 4);
        _resultsTabs.Name = "_resultsTabs";
        _resultsTabs.SelectedIndex = 0;
        _resultsTabs.Size = new Size(722, 284);
        _resultsTabs.TabIndex = 14;
        // 
        // _accountsTab
        // 
        _accountsTab.Controls.Add(_accountsGrid);
        _accountsTab.Location = new Point(4, 29);
        _accountsTab.Margin = new Padding(3, 4, 3, 4);
        _accountsTab.Name = "_accountsTab";
        _accountsTab.Size = new Size(714, 287);
        _accountsTab.TabIndex = 0;
        _accountsTab.Text = "Working accounts";
        _accountsTab.UseVisualStyleBackColor = true;
        // 
        // _accountsGrid
        // 
        _accountsGrid.AllowUserToAddRows = false;
        _accountsGrid.AllowUserToDeleteRows = false;
        _accountsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _accountsGrid.ColumnHeadersHeight = 29;
        _accountsGrid.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
        _accountsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Passed by", Name = "dataGridViewTextBoxColumn5", ReadOnly = true });
        _accountsGrid.Dock = DockStyle.Fill;
        _accountsGrid.Location = new Point(0, 0);
        _accountsGrid.Margin = new Padding(3, 4, 3, 4);
        _accountsGrid.MultiSelect = false;
        _accountsGrid.Name = "_accountsGrid";
        _accountsGrid.ReadOnly = true;
        _accountsGrid.RowHeadersWidth = 51;
        _accountsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _accountsGrid.Size = new Size(714, 287);
        _accountsGrid.TabIndex = 0;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.HeaderText = "Server";
        dataGridViewTextBoxColumn1.MinimumWidth = 6;
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewTextBoxColumn2.HeaderText = "User ID";
        dataGridViewTextBoxColumn2.MinimumWidth = 6;
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        dataGridViewTextBoxColumn2.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn3
        // 
        dataGridViewTextBoxColumn3.HeaderText = "Password";
        dataGridViewTextBoxColumn3.MinimumWidth = 6;
        dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
        dataGridViewTextBoxColumn3.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn4
        // 
        dataGridViewTextBoxColumn4.HeaderText = "Channels";
        dataGridViewTextBoxColumn4.MinimumWidth = 6;
        dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        dataGridViewTextBoxColumn4.ReadOnly = true;
        // 
        // _detailsTab
        // 
        _detailsTab.Controls.Add(_details);
        _detailsTab.Location = new Point(4, 29);
        _detailsTab.Margin = new Padding(3, 4, 3, 4);
        _detailsTab.Name = "_detailsTab";
        _detailsTab.Size = new Size(714, 251);
        _detailsTab.TabIndex = 1;
        _detailsTab.Text = "Details / test output";
        _detailsTab.UseVisualStyleBackColor = true;
        // 
        // _fullUrlLabel
        // 
        _fullUrlLabel.AutoSize = true;
        _fullUrlLabel.Location = new Point(18, 129);
        _fullUrlLabel.Name = "_fullUrlLabel";
        _fullUrlLabel.Size = new Size(70, 20);
        _fullUrlLabel.TabIndex = 1;
        _fullUrlLabel.Text = "M3U URL";
        // 
        // _serverLabel
        // 
        _serverLabel.AutoSize = true;
        _serverLabel.Location = new Point(18, 177);
        _serverLabel.Name = "_serverLabel";
        _serverLabel.Size = new Size(80, 20);
        _serverLabel.TabIndex = 2;
        _serverLabel.Text = "Server URL";
        // 
        // _usernameLabel
        // 
        _usernameLabel.AutoSize = true;
        _usernameLabel.Location = new Point(18, 225);
        _usernameLabel.Name = "_usernameLabel";
        _usernameLabel.Size = new Size(75, 20);
        _usernameLabel.TabIndex = 3;
        _usernameLabel.Text = "Username";
        // 
        // _passwordLabel
        // 
        _passwordLabel.AutoSize = true;
        _passwordLabel.Location = new Point(18, 273);
        _passwordLabel.Name = "_passwordLabel";
        _passwordLabel.Size = new Size(70, 20);
        _passwordLabel.TabIndex = 4;
        _passwordLabel.Text = "Password";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(759, 723);
        Controls.Add(_modeGroup);
        Controls.Add(_fullUrlLabel);
        Controls.Add(_serverLabel);
        Controls.Add(_usernameLabel);
        Controls.Add(_passwordLabel);
        Controls.Add(_fullUrlInput);
        Controls.Add(_serverInput);
        Controls.Add(_usernameInput);
        Controls.Add(_passwordInput);
        Controls.Add(_loadButton);
        Controls.Add(_testStreamButton);
        Controls.Add(_importFileButton);
        Controls.Add(_saveCsvButton);
        Controls.Add(_status);
        Controls.Add(_resultsTabs);
        Margin = new Padding(3, 4, 3, 4);
        MinimumSize = new Size(637, 624);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Xtream Server Details";
        _modeGroup.ResumeLayout(false);
        _modeGroup.PerformLayout();
        _resultsTabs.ResumeLayout(false);
        _accountsTab.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_accountsGrid).EndInit();
        _detailsTab.ResumeLayout(false);
        _detailsTab.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
}
