namespace EmailLLMGateway.Services
{
    /// <summary>
    /// Interface for LLM provider implementations
    /// </summary>
    public interface ILLMProvider
    {
        /// <summary>
        /// Gets a response from the LLM provider for the given prompt
        /// </summary>
        /// <param name="prompt">The user prompt to send to the LLM</param>
        /// <returns>The LLM's response text</returns>
        Task<string> GetResponseAsync(string prompt);

        /// <summary>
        /// Validates that the provider configuration is valid
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise</returns>
        bool ValidateConfiguration();
    }
}
