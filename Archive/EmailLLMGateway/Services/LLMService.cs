using EmailLLMGateway.Models;

namespace EmailLLMGateway.Services
{
    public class LLMService
    {
        private readonly LLMSettings _settings;
        private readonly ILLMProvider _provider;

        public LLMService(LLMSettings settings)
        {
            _settings = settings;
            _provider = CreateProvider(settings);
        }

        private ILLMProvider CreateProvider(LLMSettings settings)
        {
            return settings.Provider.ToLower() switch
            {
                "anthropic" => new AnthropicProvider(settings),
                "openai" => new OpenAIProvider(settings),
                _ => throw new NotSupportedException($"Provider '{settings.Provider}' is not supported. Supported providers: Anthropic, OpenAI")
            };
        }

        public async Task<string> GetLLMResponseAsync(string prompt)
        {
            try
            {
                return await _provider.GetResponseAsync(prompt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting LLM response from {_settings.Provider}: {ex.Message}", ex);
            }
        }

        public bool ValidateConfiguration()
        {
            return _provider.ValidateConfiguration();
        }
    }
}
