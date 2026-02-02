namespace EmailLLMGateway.Models
{
    public class AppConfiguration
    {
        public EmailSettings EmailSettings { get; set; } = new EmailSettings();
        public LLMSettings LLMSettings { get; set; } = new LLMSettings();
        public MonitoringSettings MonitoringSettings { get; set; } = new MonitoringSettings();
    }

    public class EmailSettings
    {
        // IMAP Settings
        public string ImapServer { get; set; } = "";
        public int ImapPort { get; set; } = 993;
        public bool ImapUseSsl { get; set; } = true;
        public string EmailAddress { get; set; } = "";
        public string Password { get; set; } = "";

        // SMTP Settings
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public bool SmtpUseSsl { get; set; } = true;
        public string SmtpUsername { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
    }

    public class LLMSettings
    {
        public string Provider { get; set; } = "Anthropic"; // Anthropic, OpenAI, etc.
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "claude-3-5-sonnet-20241022";
        public int MaxTokens { get; set; } = 4096;
        public string BaseUrl { get; set; } = ""; // Optional: Custom base URL for OpenAI-compatible APIs
    }

    public class MonitoringSettings
    {
        public string SubjectPattern { get; set; } = "[LLM]"; // Subject must contain this to trigger
        public int PollingIntervalSeconds { get; set; } = 30;
        public bool AutoStart { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
    }
}
