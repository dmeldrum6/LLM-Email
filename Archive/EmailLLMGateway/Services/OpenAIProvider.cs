using EmailLLMGateway.Models;
using OpenAI;
using OpenAI.Chat;

namespace EmailLLMGateway.Services
{
    /// <summary>
    /// LLM provider implementation for OpenAI-compatible APIs
    /// Supports OpenAI, Azure OpenAI, and other OpenAI-compatible endpoints
    /// </summary>
    public class OpenAIProvider : ILLMProvider
    {
        private readonly LLMSettings _settings;

        public OpenAIProvider(LLMSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> GetResponseAsync(string prompt)
        {
            // Create OpenAI client configuration
            OpenAIClientOptions? options = null;

            // If a custom base URL is provided, use it (for Azure OpenAI or other compatible APIs)
            if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                options = new OpenAIClientOptions
                {
                    Endpoint = new Uri(_settings.BaseUrl)
                };
            }

            // Create the OpenAI client
            var client = options != null
                ? new OpenAIClient(_settings.ApiKey, options)
                : new OpenAIClient(_settings.ApiKey);

            // Build the chat completion request
            var messages = new List<ChatMessage>
            {
                new UserChatMessage(prompt)
            };

            var chatClient = client.GetChatClient(_settings.Model);

            var completion = await chatClient.CompleteChatAsync(
                messages,
                new ChatCompletionOptions
                {
                    MaxOutputTokenCount = _settings.MaxTokens
                });

            if (completion?.Value?.Content != null && completion.Value.Content.Count > 0)
            {
                return completion.Value.Content[0].Text ?? "No response generated.";
            }

            return "No response generated.";
        }

        public bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                return false;

            if (string.IsNullOrWhiteSpace(_settings.Model))
                return false;

            if (_settings.MaxTokens <= 0)
                return false;

            // Validate BaseUrl if provided
            if (!string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                if (!Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out _))
                    return false;
            }

            return true;
        }
    }
}
