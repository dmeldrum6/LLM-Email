using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using EmailLLMGateway.Models;

namespace EmailLLMGateway.Services
{
    public class LLMService
    {
        private readonly LLMSettings _settings;

        public LLMService(LLMSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> GetLLMResponseAsync(string prompt)
        {
            try
            {
                if (_settings.Provider == "Anthropic")
                {
                    return await GetAnthropicResponseAsync(prompt);
                }
                else
                {
                    throw new NotImplementedException($"Provider '{_settings.Provider}' is not yet implemented.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting LLM response: {ex.Message}", ex);
            }
        }

        private async Task<string> GetAnthropicResponseAsync(string prompt)
        {
            var client = new AnthropicClient(new APIAuthentication(_settings.ApiKey));

            var messages = new List<Message>
            {
                new Message(RoleType.User, prompt)
            };

            var parameters = new MessageParameters
            {
                Messages = messages,
                Model = _settings.Model,
                MaxTokens = _settings.MaxTokens,
                Stream = false
            };

            var response = await client.Messages.GetClaudeMessageAsync(parameters);

            if (response?.Content != null && response.Content.Count > 0)
            {
                return response.Content[0].Text ?? "No response generated.";
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

            return true;
        }
    }
}
