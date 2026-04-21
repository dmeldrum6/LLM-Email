namespace EmailLLMResponder.Models
{
    public class LLMConfig
    {
        public string ApiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-3.5-turbo";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 500;
        public string SystemPrompt { get; set; } = "You are a helpful email assistant. Respond to emails in a professional and concise manner.";
        public bool EnableRefinementLoop { get; set; } = false;
        public int RefinementPasses { get; set; } = 2;
    }
}
