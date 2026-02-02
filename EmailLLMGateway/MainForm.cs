using EmailLLMGateway.Models;
using EmailLLMGateway.Services;

namespace EmailLLMGateway
{
    public partial class MainForm : Form
    {
        private AppConfiguration _config;
        private readonly LoggingService _loggingService;
        private MonitoringService? _monitoringService;
        private NotifyIcon? _trayIcon;

        public MainForm()
        {
            InitializeComponent();
            _loggingService = new LoggingService();
            _config = ConfigurationManager.LoadConfiguration();

            InitializeUI();
            SetupTrayIcon();
            LoadConfigurationToUI();

            _loggingService.LogEntryAdded += OnLogEntryAdded;
        }

        private void InitializeUI()
        {
            this.Text = "Email LLM Gateway - Configuration";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += MainForm_FormClosing;
            this.Resize += MainForm_Resize;

            CreateControls();
        }

        private void CreateControls()
        {
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Email Settings Tab
            var emailTab = new TabPage("Email Settings");
            CreateEmailSettingsTab(emailTab);
            tabControl.TabPages.Add(emailTab);

            // LLM Settings Tab
            var llmTab = new TabPage("LLM Settings");
            CreateLLMSettingsTab(llmTab);
            tabControl.TabPages.Add(llmTab);

            // Monitoring Tab
            var monitoringTab = new TabPage("Monitoring");
            CreateMonitoringTab(monitoringTab);
            tabControl.TabPages.Add(monitoringTab);

            // Log Tab
            var logTab = new TabPage("Log");
            CreateLogTab(logTab);
            tabControl.TabPages.Add(logTab);

            this.Controls.Add(tabControl);
        }

        private TextBox? txtImapServer, txtImapPort, txtEmailAddress, txtEmailPassword;
        private TextBox? txtSmtpServer, txtSmtpPort, txtSmtpUsername, txtSmtpPassword;
        private CheckBox? chkImapSsl, chkSmtpSsl;

        private void CreateEmailSettingsTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            int y = 10;

            // IMAP Settings
            AddLabel(panel, "IMAP Server:", 10, y);
            txtImapServer = AddTextBox(panel, 150, y, 400);
            y += 35;

            AddLabel(panel, "IMAP Port:", 10, y);
            txtImapPort = AddTextBox(panel, 150, y, 100);
            chkImapSsl = AddCheckBox(panel, "Use SSL", 270, y);
            y += 35;

            AddLabel(panel, "Email Address:", 10, y);
            txtEmailAddress = AddTextBox(panel, 150, y, 400);
            y += 35;

            AddLabel(panel, "Password:", 10, y);
            txtEmailPassword = AddTextBox(panel, 150, y, 400, true);
            y += 45;

            // SMTP Settings
            AddLabel(panel, "SMTP Server:", 10, y, true);
            txtSmtpServer = AddTextBox(panel, 150, y, 400);
            y += 35;

            AddLabel(panel, "SMTP Port:", 10, y);
            txtSmtpPort = AddTextBox(panel, 150, y, 100);
            chkSmtpSsl = AddCheckBox(panel, "Use SSL", 270, y);
            y += 35;

            AddLabel(panel, "SMTP Username:", 10, y);
            txtSmtpUsername = AddTextBox(panel, 150, y, 400);
            AddLabel(panel, "(leave empty to use email address)", 560, y, false, 8);
            y += 35;

            AddLabel(panel, "SMTP Password:", 10, y);
            txtSmtpPassword = AddTextBox(panel, 150, y, 400, true);
            AddLabel(panel, "(leave empty to use email password)", 560, y, false, 8);
            y += 45;

            var btnSave = new Button { Text = "Save Settings", Left = 150, Top = y, Width = 120 };
            btnSave.Click += BtnSaveEmail_Click;
            panel.Controls.Add(btnSave);

            tab.Controls.Add(panel);
        }

        private TextBox? txtApiKey, txtModel, txtMaxTokens;
        private ComboBox? cmbProvider;

        private void CreateLLMSettingsTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            int y = 10;

            AddLabel(panel, "Provider:", 10, y);
            cmbProvider = new ComboBox { Left = 150, Top = y, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProvider.Items.AddRange(new object[] { "Anthropic" });
            cmbProvider.SelectedIndex = 0;
            panel.Controls.Add(cmbProvider);
            y += 35;

            AddLabel(panel, "API Key:", 10, y);
            txtApiKey = AddTextBox(panel, 150, y, 400, true);
            y += 35;

            AddLabel(panel, "Model:", 10, y);
            txtModel = AddTextBox(panel, 150, y, 400);
            AddLabel(panel, "(e.g., claude-3-5-sonnet-20241022)", 560, y, false, 8);
            y += 35;

            AddLabel(panel, "Max Tokens:", 10, y);
            txtMaxTokens = AddTextBox(panel, 150, y, 100);
            y += 45;

            var btnSave = new Button { Text = "Save Settings", Left = 150, Top = y, Width = 120 };
            btnSave.Click += BtnSaveLLM_Click;
            panel.Controls.Add(btnSave);

            tab.Controls.Add(panel);
        }

        private TextBox? txtSubjectPattern, txtPollingInterval;
        private CheckBox? chkAutoStart, chkMinimizeToTray;
        private Button? btnStartStop;
        private Label? lblStatus;

        private void CreateMonitoringTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            int y = 10;

            AddLabel(panel, "Subject Pattern:", 10, y);
            txtSubjectPattern = AddTextBox(panel, 150, y, 200);
            AddLabel(panel, "(Emails with this in subject will be processed)", 360, y, false, 8);
            y += 35;

            AddLabel(panel, "Polling Interval:", 10, y);
            txtPollingInterval = AddTextBox(panel, 150, y, 100);
            AddLabel(panel, "seconds", 260, y);
            y += 35;

            chkAutoStart = AddCheckBox(panel, "Auto-start monitoring on launch", 150, y);
            y += 30;

            chkMinimizeToTray = AddCheckBox(panel, "Minimize to system tray when closed", 150, y);
            y += 45;

            var btnSave = new Button { Text = "Save Settings", Left = 150, Top = y, Width = 120 };
            btnSave.Click += BtnSaveMonitoring_Click;
            panel.Controls.Add(btnSave);
            y += 45;

            // Status and control section
            AddLabel(panel, "Status:", 10, y, true);
            y += 25;

            lblStatus = new Label
            {
                Left = 10,
                Top = y,
                Width = 660,
                Height = 60,
                Text = "Monitoring service is stopped",
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            panel.Controls.Add(lblStatus);
            y += 70;

            btnStartStop = new Button { Text = "Start Monitoring", Left = 10, Top = y, Width = 150, Height = 35 };
            btnStartStop.Click += BtnStartStop_Click;
            panel.Controls.Add(btnStartStop);

            tab.Controls.Add(panel);
        }

        private ListBox? lstLog;

        private void CreateLogTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            lstLog = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                HorizontalScrollbar = true
            };
            panel.Controls.Add(lstLog);

            var btnClear = new Button
            {
                Text = "Clear Log",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            btnClear.Click += (s, e) => lstLog.Items.Clear();
            panel.Controls.Add(btnClear);

            // Load recent logs
            foreach (var log in _loggingService.GetRecentLogs())
            {
                lstLog.Items.Add(log);
            }

            tab.Controls.Add(panel);
        }

        private Label AddLabel(Panel panel, string text, int x, int y, bool bold = false, int fontSize = 9)
        {
            var label = new Label
            {
                Text = text,
                Left = x,
                Top = y,
                Width = 130,
                Font = bold ? new Font(Font.FontFamily, fontSize, FontStyle.Bold) : new Font(Font.FontFamily, fontSize)
            };
            panel.Controls.Add(label);
            return label;
        }

        private TextBox AddTextBox(Panel panel, int x, int y, int width, bool password = false)
        {
            var textBox = new TextBox
            {
                Left = x,
                Top = y,
                Width = width,
                UseSystemPasswordChar = password
            };
            panel.Controls.Add(textBox);
            return textBox;
        }

        private CheckBox AddCheckBox(Panel panel, string text, int x, int y)
        {
            var checkBox = new CheckBox
            {
                Text = text,
                Left = x,
                Top = y,
                Width = 300
            };
            panel.Controls.Add(checkBox);
            return checkBox;
        }

        private void LoadConfigurationToUI()
        {
            // Email settings
            if (txtImapServer != null) txtImapServer.Text = _config.EmailSettings.ImapServer;
            if (txtImapPort != null) txtImapPort.Text = _config.EmailSettings.ImapPort.ToString();
            if (chkImapSsl != null) chkImapSsl.Checked = _config.EmailSettings.ImapUseSsl;
            if (txtEmailAddress != null) txtEmailAddress.Text = _config.EmailSettings.EmailAddress;
            if (txtEmailPassword != null) txtEmailPassword.Text = _config.EmailSettings.Password;

            if (txtSmtpServer != null) txtSmtpServer.Text = _config.EmailSettings.SmtpServer;
            if (txtSmtpPort != null) txtSmtpPort.Text = _config.EmailSettings.SmtpPort.ToString();
            if (chkSmtpSsl != null) chkSmtpSsl.Checked = _config.EmailSettings.SmtpUseSsl;
            if (txtSmtpUsername != null) txtSmtpUsername.Text = _config.EmailSettings.SmtpUsername;
            if (txtSmtpPassword != null) txtSmtpPassword.Text = _config.EmailSettings.SmtpPassword;

            // LLM settings
            if (cmbProvider != null) cmbProvider.SelectedItem = _config.LLMSettings.Provider;
            if (txtApiKey != null) txtApiKey.Text = _config.LLMSettings.ApiKey;
            if (txtModel != null) txtModel.Text = _config.LLMSettings.Model;
            if (txtMaxTokens != null) txtMaxTokens.Text = _config.LLMSettings.MaxTokens.ToString();

            // Monitoring settings
            if (txtSubjectPattern != null) txtSubjectPattern.Text = _config.MonitoringSettings.SubjectPattern;
            if (txtPollingInterval != null) txtPollingInterval.Text = _config.MonitoringSettings.PollingIntervalSeconds.ToString();
            if (chkAutoStart != null) chkAutoStart.Checked = _config.MonitoringSettings.AutoStart;
            if (chkMinimizeToTray != null) chkMinimizeToTray.Checked = _config.MonitoringSettings.MinimizeToTray;
        }

        private void BtnSaveEmail_Click(object? sender, EventArgs e)
        {
            _config.EmailSettings.ImapServer = txtImapServer?.Text ?? "";
            _config.EmailSettings.ImapPort = int.TryParse(txtImapPort?.Text, out int imapPort) ? imapPort : 993;
            _config.EmailSettings.ImapUseSsl = chkImapSsl?.Checked ?? true;
            _config.EmailSettings.EmailAddress = txtEmailAddress?.Text ?? "";
            _config.EmailSettings.Password = txtEmailPassword?.Text ?? "";

            _config.EmailSettings.SmtpServer = txtSmtpServer?.Text ?? "";
            _config.EmailSettings.SmtpPort = int.TryParse(txtSmtpPort?.Text, out int smtpPort) ? smtpPort : 587;
            _config.EmailSettings.SmtpUseSsl = chkSmtpSsl?.Checked ?? true;
            _config.EmailSettings.SmtpUsername = txtSmtpUsername?.Text ?? "";
            _config.EmailSettings.SmtpPassword = txtSmtpPassword?.Text ?? "";

            ConfigurationManager.SaveConfiguration(_config);
            MessageBox.Show("Email settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSaveLLM_Click(object? sender, EventArgs e)
        {
            _config.LLMSettings.Provider = cmbProvider?.SelectedItem?.ToString() ?? "Anthropic";
            _config.LLMSettings.ApiKey = txtApiKey?.Text ?? "";
            _config.LLMSettings.Model = txtModel?.Text ?? "";
            _config.LLMSettings.MaxTokens = int.TryParse(txtMaxTokens?.Text, out int maxTokens) ? maxTokens : 4096;

            ConfigurationManager.SaveConfiguration(_config);
            MessageBox.Show("LLM settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSaveMonitoring_Click(object? sender, EventArgs e)
        {
            _config.MonitoringSettings.SubjectPattern = txtSubjectPattern?.Text ?? "[LLM]";
            _config.MonitoringSettings.PollingIntervalSeconds = int.TryParse(txtPollingInterval?.Text, out int interval) ? interval : 30;
            _config.MonitoringSettings.AutoStart = chkAutoStart?.Checked ?? false;
            _config.MonitoringSettings.MinimizeToTray = chkMinimizeToTray?.Checked ?? true;

            ConfigurationManager.SaveConfiguration(_config);
            MessageBox.Show("Monitoring settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnStartStop_Click(object? sender, EventArgs e)
        {
            if (_monitoringService?.IsRunning == true)
            {
                _monitoringService.Stop();
                if (btnStartStop != null) btnStartStop.Text = "Start Monitoring";
            }
            else
            {
                StartMonitoring();
            }
        }

        private void StartMonitoring()
        {
            // Reload configuration
            _config = ConfigurationManager.LoadConfiguration();

            // Validate configuration
            if (string.IsNullOrWhiteSpace(_config.EmailSettings.ImapServer) ||
                string.IsNullOrWhiteSpace(_config.EmailSettings.EmailAddress))
            {
                MessageBox.Show("Please configure email settings first.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_config.LLMSettings.ApiKey))
            {
                MessageBox.Show("Please configure LLM settings first.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _monitoringService = new MonitoringService(_config, _loggingService);
            _monitoringService.StatusChanged += OnStatusChanged;
            _monitoringService.EmailProcessed += OnEmailProcessed;
            _monitoringService.Start();

            if (btnStartStop != null) btnStartStop.Text = "Stop Monitoring";
        }

        private void OnStatusChanged(object? sender, string status)
        {
            if (lblStatus != null && lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(() => lblStatus.Text = status);
            }
            else if (lblStatus != null)
            {
                lblStatus.Text = status;
            }
        }

        private void OnEmailProcessed(object? sender, EmailProcessedEventArgs e)
        {
            string message = e.Success
                ? $"Successfully processed email from {e.From}"
                : $"Failed to process email from {e.From}: {e.ErrorMessage}";

            _loggingService.LogInfo(message);
        }

        private void OnLogEntryAdded(object? sender, string logEntry)
        {
            if (lstLog != null && lstLog.InvokeRequired)
            {
                lstLog.Invoke(() =>
                {
                    lstLog.Items.Add(logEntry);
                    lstLog.TopIndex = lstLog.Items.Count - 1;
                });
            }
            else if (lstLog != null)
            {
                lstLog.Items.Add(logEntry);
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Text = "Email LLM Gateway",
                Visible = false
            };

            // You would need to add an icon file to the project
            // _trayIcon.Icon = new Icon("app.ico");

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; _trayIcon!.Visible = false; });
            contextMenu.Items.Add("Exit", null, (s, e) => { Application.Exit(); });
            _trayIcon.ContextMenuStrip = contextMenu;

            _trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; _trayIcon.Visible = false; };
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && (_config.MonitoringSettings.MinimizeToTray || chkMinimizeToTray?.Checked == true))
            {
                this.Hide();
                if (_trayIcon != null) _trayIcon.Visible = true;
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_monitoringService?.IsRunning == true)
            {
                var result = MessageBox.Show(
                    "Monitoring service is still running. Do you want to stop it and exit?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _monitoringService.Stop();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _trayIcon?.Dispose();
                _monitoringService?.Stop();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(700, 650);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }
    }
}
