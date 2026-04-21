using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using EmailLLMResponder.Models;
using EmailLLMResponder.Services;

namespace EmailLLMResponder
{
    public partial class MainWindow : Window
    {
        private readonly ConfigService _configService;
        private readonly EmailService _emailService;
        private readonly LLMService _llmService;
        private AppConfig _config;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            _configService = new ConfigService();
            _emailService = new EmailService();
            _llmService = new LLMService();
            _config = _configService.LoadConfig();
            LoadConfigToUI();
            LogMessage("Application started. Configure email and LLM settings, then start monitoring.");
        }

        private void LoadConfigToUI()
        {
            ImapServerTextBox.Text = _config.EmailConfig.ImapServer;
            ImapPortTextBox.Text = _config.EmailConfig.ImapPort.ToString();
            ImapUseSslCheckBox.IsChecked = _config.EmailConfig.ImapUseSsl;
            SmtpServerTextBox.Text = _config.EmailConfig.SmtpServer;
            SmtpPortTextBox.Text = _config.EmailConfig.SmtpPort.ToString();
            SmtpUseSslCheckBox.IsChecked = _config.EmailConfig.SmtpUseSsl;
            EmailAddressTextBox.Text = _config.EmailConfig.EmailAddress;
            EmailPasswordBox.Password = _config.EmailConfig.Password;
            CheckIntervalTextBox.Text = _config.EmailConfig.CheckIntervalSeconds.ToString();
            SubjectFilterTextBox.Text = _config.EmailConfig.SubjectFilter;

            ApiEndpointTextBox.Text = _config.LLMConfig.ApiEndpoint;
            ApiKeyBox.Password = _config.LLMConfig.ApiKey;
            ModelTextBox.Text = _config.LLMConfig.Model;
            TemperatureSlider.Value = _config.LLMConfig.Temperature;
            MaxTokensTextBox.Text = _config.LLMConfig.MaxTokens.ToString();
            SystemPromptTextBox.Text = _config.LLMConfig.SystemPrompt;
            EnableRefinementLoopCheckBox.IsChecked = _config.LLMConfig.EnableRefinementLoop;
            RefinementPassesTextBox.Text = _config.LLMConfig.RefinementPasses.ToString();
            RefinementPassesTextBox.IsEnabled = _config.LLMConfig.EnableRefinementLoop;
        }

        private void SaveConfigFromUI()
        {
            if (int.TryParse(ImapPortTextBox.Text, out int imapPort))
                _config.EmailConfig.ImapPort = imapPort;

            if (int.TryParse(SmtpPortTextBox.Text, out int smtpPort))
                _config.EmailConfig.SmtpPort = smtpPort;

            if (int.TryParse(CheckIntervalTextBox.Text, out int interval))
                _config.EmailConfig.CheckIntervalSeconds = interval;

            _config.EmailConfig.SubjectFilter = SubjectFilterTextBox.Text;

            if (int.TryParse(MaxTokensTextBox.Text, out int maxTokens))
                _config.LLMConfig.MaxTokens = maxTokens;

            _config.EmailConfig.ImapServer = ImapServerTextBox.Text;
            _config.EmailConfig.ImapUseSsl = ImapUseSslCheckBox.IsChecked ?? true;
            _config.EmailConfig.SmtpServer = SmtpServerTextBox.Text;
            _config.EmailConfig.SmtpUseSsl = SmtpUseSslCheckBox.IsChecked ?? true;
            _config.EmailConfig.EmailAddress = EmailAddressTextBox.Text;
            _config.EmailConfig.Password = EmailPasswordBox.Password;

            _config.LLMConfig.ApiEndpoint = ApiEndpointTextBox.Text;
            _config.LLMConfig.ApiKey = ApiKeyBox.Password;
            _config.LLMConfig.Model = ModelTextBox.Text;
            _config.LLMConfig.Temperature = TemperatureSlider.Value;
            _config.LLMConfig.SystemPrompt = SystemPromptTextBox.Text;
            _config.LLMConfig.EnableRefinementLoop = EnableRefinementLoopCheckBox.IsChecked ?? false;
            if (int.TryParse(RefinementPassesTextBox.Text, out int refinementPasses) && refinementPasses >= 1 && refinementPasses <= 5)
                _config.LLMConfig.RefinementPasses = refinementPasses;
        }

        private async void TestEmailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusBarText.Text = "Testing email connection...";
                SaveConfigFromUI();

                await _emailService.TestConnectionAsync(_config.EmailConfig);

                MessageBox.Show("Email connection successful!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LogMessage("Email connection test: SUCCESS");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Connection Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"Email connection test FAILED: {ex.Message}");
            }
            finally
            {
                StatusBarText.Text = "Ready";
            }
        }

        private void SaveEmailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveConfigFromUI();
                _configService.SaveConfig(_config);
                MessageBox.Show("Email configuration saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LogMessage("Email configuration saved");
                StatusBarText.Text = "Email configuration saved";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving email configuration: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"Error saving email configuration: {ex.Message}");
            }
        }

        private async void TestLLMButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusBarText.Text = "Testing LLM connection...";
                SaveConfigFromUI();

                bool success = await _llmService.TestConnectionAsync(_config.LLMConfig);

                if (success)
                {
                    MessageBox.Show("LLM connection successful!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LogMessage("LLM connection test: SUCCESS");
                }
                else
                {
                    MessageBox.Show("LLM connection failed. Please check your settings.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    LogMessage("LLM connection test: FAILED");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing LLM connection: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"LLM connection test error: {ex.Message}");
            }
            finally
            {
                StatusBarText.Text = "Ready";
            }
        }

        private void SaveLLMButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveConfigFromUI();
                _configService.SaveConfig(_config);
                MessageBox.Show("LLM configuration saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LogMessage("LLM configuration saved");
                StatusBarText.Text = "LLM configuration saved";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving LLM configuration: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"Error saving LLM configuration: {ex.Message}");
            }
        }

        private void EnableRefinementLoopCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (RefinementPassesTextBox != null)
                RefinementPassesTextBox.IsEnabled = EnableRefinementLoopCheckBox.IsChecked ?? false;
        }

        private void TemperatureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TemperatureValue != null)
            {
                TemperatureValue.Text = TemperatureSlider.Value.ToString("F1");
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            SaveConfigFromUI();

            if (string.IsNullOrWhiteSpace(_config.EmailConfig.EmailAddress) ||
                string.IsNullOrWhiteSpace(_config.EmailConfig.Password))
            {
                MessageBox.Show("Please configure email settings first.", "Configuration Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MainTabControl.SelectedIndex = 0;
                return;
            }

            if (string.IsNullOrWhiteSpace(_config.LLMConfig.ApiKey))
            {
                MessageBox.Show("Please configure LLM settings first.", "Configuration Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MainTabControl.SelectedIndex = 1;
                return;
            }

            _isRunning = true;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            StatusTextBlock.Text = "Status: Running";
            LogMessage("=== Monitoring Started ===");

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => MonitorEmailsAsync(_cancellationTokenSource.Token));
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopMonitoring();
        }

        private void StopMonitoring()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StatusTextBlock.Text = "Status: Stopped";
            LogMessage("=== Monitoring Stopped ===");
        }

        private async Task MonitorEmailsAsync(CancellationToken cancellationToken)
        {
            LogMessage($"Checking for emails every {_config.EmailConfig.CheckIntervalSeconds} seconds...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var emails = await _emailService.GetUnreadEmailsAsync(_config.EmailConfig);

                    if (emails.Count > 0)
                    {
                        LogMessage($"Found {emails.Count} unread email(s)");

                        foreach (var email in emails)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            var subjectFilter = _config.EmailConfig.SubjectFilter;
                            if (!string.IsNullOrWhiteSpace(subjectFilter) &&
                                !email.Subject.Contains(subjectFilter, StringComparison.OrdinalIgnoreCase))
                            {
                                LogMessage($"Skipping email (subject filter not matched): {email.Subject}");
                                await _emailService.MarkAsReadAsync(email.Uid, _config.EmailConfig);
                                continue;
                            }

                            await ProcessEmailAsync(email);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_config.EmailConfig.CheckIntervalSeconds), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogMessage($"Error in monitoring loop: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
            }
        }

        private async Task ProcessEmailAsync(EmailMessage email)
        {
            try
            {
                LogMessage($"\n--- Processing Email ---");
                LogMessage($"From: {email.FromName} <{email.From}>");
                LogMessage($"Subject: {email.Subject}");
                LogMessage($"Received: {email.ReceivedDate}");

                LogMessage("Generating LLM response...");
                var llmResponse = await _llmService.GetResponseAsync(
                    $"Subject: {email.Subject}\n\nMessage: {email.Body}",
                    _config.LLMConfig,
                    LogMessage);

                LogMessage($"LLM Response: {llmResponse.Substring(0, Math.Min(100, llmResponse.Length))}...");

                LogMessage("Sending reply email...");
                var replySubject = email.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase)
                    ? email.Subject
                    : $"Re: {email.Subject}";

                await _emailService.SendEmailAsync(email.From, replySubject, llmResponse, _config.EmailConfig);

                LogMessage("Marking email as read...");
                await _emailService.MarkAsReadAsync(email.Uid, _config.EmailConfig);

                LogMessage($"Successfully processed and replied to email from {email.From}");
                LogMessage("--- Email Processing Complete ---\n");
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing email from {email.From}: {ex.Message}");
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                LogTextBox.AppendText($"[{timestamp}] {message}\n");
                LogScrollViewer.ScrollToBottom();
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_isRunning)
            {
                var result = MessageBox.Show(
                    "Monitoring is still running. Are you sure you want to exit?",
                    "Confirm Exit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                StopMonitoring();
            }

            base.OnClosing(e);
        }
    }
}
