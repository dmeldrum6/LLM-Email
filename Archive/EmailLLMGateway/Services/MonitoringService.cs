using EmailLLMGateway.Models;
using System.Timers;

namespace EmailLLMGateway.Services
{
    public class MonitoringService
    {
        private readonly AppConfiguration _config;
        private readonly EmailService _emailService;
        private readonly LLMService _llmService;
        private readonly LoggingService _loggingService;
        private System.Timers.Timer? _monitoringTimer;
        private bool _isProcessing = false;

        public event EventHandler<string>? StatusChanged;
        public event EventHandler<EmailProcessedEventArgs>? EmailProcessed;

        public bool IsRunning { get; private set; }

        public MonitoringService(AppConfiguration config, LoggingService loggingService)
        {
            _config = config;
            _emailService = new EmailService(config.EmailSettings);
            _llmService = new LLMService(config.LLMSettings);
            _loggingService = loggingService;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            _loggingService.LogInfo("Starting email monitoring service...");
            UpdateStatus("Starting monitoring service...");

            _monitoringTimer = new System.Timers.Timer(_config.MonitoringSettings.PollingIntervalSeconds * 1000);
            _monitoringTimer.Elapsed += OnTimerElapsed;
            _monitoringTimer.AutoReset = true;
            _monitoringTimer.Start();

            IsRunning = true;
            _loggingService.LogInfo("Monitoring service started successfully");
            UpdateStatus("Monitoring service running");

            // Do an immediate check
            Task.Run(() => CheckEmailsAsync());
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            _loggingService.LogInfo("Stopping email monitoring service...");
            UpdateStatus("Stopping monitoring service...");

            _monitoringTimer?.Stop();
            _monitoringTimer?.Dispose();
            _monitoringTimer = null;

            IsRunning = false;
            _loggingService.LogInfo("Monitoring service stopped");
            UpdateStatus("Monitoring service stopped");
        }

        private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            await CheckEmailsAsync();
        }

        private async Task CheckEmailsAsync()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;

            try
            {
                _loggingService.LogInfo("Checking for new emails...");
                UpdateStatus("Checking for new emails...");

                var emails = await _emailService.CheckForNewEmailsAsync(_config.MonitoringSettings.SubjectPattern);

                if (emails.Count > 0)
                {
                    _loggingService.LogInfo($"Found {emails.Count} new email(s) to process");
                    UpdateStatus($"Processing {emails.Count} email(s)...");

                    foreach (var email in emails)
                    {
                        await ProcessEmailAsync(email);
                    }
                }

                UpdateStatus(IsRunning ? "Monitoring service running" : "Monitoring service stopped");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error checking emails", ex);
                UpdateStatus("Error checking emails - see log for details");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Task ProcessEmailAsync(EmailMessage email)
        {
            try
            {
                _loggingService.LogInfo($"Processing email from {email.From}: {email.Subject}");

                // Extract the prompt (remove the subject pattern)
                string prompt = email.Body;
                if (!string.IsNullOrWhiteSpace(email.Subject))
                {
                    string cleanSubject = email.Subject.Replace(_config.MonitoringSettings.SubjectPattern, "", StringComparison.OrdinalIgnoreCase).Trim();
                    if (!string.IsNullOrWhiteSpace(cleanSubject))
                    {
                        prompt = $"Subject: {cleanSubject}\n\n{prompt}";
                    }
                }

                // Get LLM response
                _loggingService.LogInfo("Sending prompt to LLM...");
                string llmResponse = await _llmService.GetLLMResponseAsync(prompt);

                // Send reply email
                _loggingService.LogInfo("Sending reply email...");
                await _emailService.SendReplyAsync(email.From, email.Subject, llmResponse, email.MessageId);

                _loggingService.LogInfo($"Successfully processed and replied to email from {email.From}");

                EmailProcessed?.Invoke(this, new EmailProcessedEventArgs
                {
                    From = email.From,
                    Subject = email.Subject,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error processing email from {email.From}", ex);

                EmailProcessed?.Invoke(this, new EmailProcessedEventArgs
                {
                    From = email.From,
                    Subject = email.Subject,
                    Success = false,
                    ErrorMessage = ex.Message
                });

                // Optionally send error notification email
                try
                {
                    await _emailService.SendReplyAsync(
                        email.From,
                        email.Subject,
                        $"An error occurred while processing your request:\n\n{ex.Message}\n\nPlease try again or contact the administrator.",
                        email.MessageId
                    );
                }
                catch
                {
                    // Ignore errors sending error notification
                }
            }
        }

        private void UpdateStatus(string status)
        {
            StatusChanged?.Invoke(this, status);
        }
    }

    public class EmailProcessedEventArgs : EventArgs
    {
        public string From { get; set; } = "";
        public string Subject { get; set; } = "";
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}
