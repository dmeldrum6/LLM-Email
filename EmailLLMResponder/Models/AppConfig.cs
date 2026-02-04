namespace EmailLLMResponder.Models
{
    public class AppConfig
    {
        public EmailConfig EmailConfig { get; set; } = new EmailConfig();
        public LLMConfig LLMConfig { get; set; } = new LLMConfig();
    }
}
