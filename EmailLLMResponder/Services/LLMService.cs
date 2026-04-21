using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EmailLLMResponder.Models;
using Newtonsoft.Json;

namespace EmailLLMResponder.Services
{
    public class LLMService
    {
        private readonly HttpClient _httpClient;

        public LLMService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<string> GetResponseAsync(string userMessage, LLMConfig config, Action<string>? logAction = null)
        {
            if (config.EnableRefinementLoop)
                return await RunRefinementLoopAsync(userMessage, config, logAction);

            return await CallApiAsync(config.SystemPrompt, userMessage, config);
        }

        private async Task<string> RunRefinementLoopAsync(string userMessage, LLMConfig config, Action<string>? logAction)
        {
            var draft = await CallApiAsync(config.SystemPrompt, userMessage, config);

            for (int pass = 1; pass <= config.RefinementPasses; pass++)
            {
                logAction?.Invoke($"Refinement pass {pass} of {config.RefinementPasses}: critiquing draft...");

                var critiqueUserMessage =
                    $"Review this email response for accuracy, completeness, and professional tone. " +
                    $"List specific issues only, no preamble.\n" +
                    $"ORIGINAL REQUEST: {userMessage}\n" +
                    $"RESPONSE TO REVIEW: {draft}";

                var critique = await CallApiAsync(
                    "You are a critical reviewer. Be concise.",
                    critiqueUserMessage,
                    config);

                logAction?.Invoke($"Refinement pass {pass} of {config.RefinementPasses}: refining response...");

                var refineUserMessage =
                    $"Original request: {userMessage}\n" +
                    $"Your draft response: {draft}\n" +
                    $"Issues identified: {critique}\n" +
                    $"Rewrite the response fixing all identified issues. " +
                    $"Reply with the improved response only, no preamble.";

                draft = await CallApiAsync(config.SystemPrompt, refineUserMessage, config);
            }

            logAction?.Invoke("Refinement complete. Sending final response.");
            return draft;
        }

        private async Task<string> CallApiAsync(string systemPrompt, string userMessage, LLMConfig config)
        {
            try
            {
                var request = new
                {
                    model = config.Model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                    temperature = config.Temperature,
                    max_tokens = config.MaxTokens
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");

                var response = await _httpClient.PostAsync(config.ApiEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"LLM API error: {response.StatusCode} - {responseContent}");
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return result?.choices?[0]?.message?.content?.ToString() ?? "No response generated.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling LLM API: {ex.Message}", ex);
            }
        }

        public async Task<bool> TestConnectionAsync(LLMConfig config)
        {
            try
            {
                await CallApiAsync(config.SystemPrompt, "Hello", config);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
