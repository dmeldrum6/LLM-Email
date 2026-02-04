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

        public async Task<string> GetResponseAsync(string userMessage, LLMConfig config)
        {
            try
            {
                var request = new
                {
                    model = config.Model,
                    messages = new[]
                    {
                        new { role = "system", content = config.SystemPrompt },
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
                await GetResponseAsync("Hello", config);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
