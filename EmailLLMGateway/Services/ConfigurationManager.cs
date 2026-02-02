using EmailLLMGateway.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace EmailLLMGateway.Services
{
    public class ConfigurationManager
    {
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EmailLLMGateway",
            "config.json"
        );

        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("EmailLLMGateway_Salt_2024");

        public static AppConfiguration LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var config = JsonConvert.DeserializeObject<AppConfiguration>(json);

                    if (config != null)
                    {
                        // Decrypt sensitive data
                        config.EmailSettings.Password = DecryptString(config.EmailSettings.Password);
                        config.EmailSettings.SmtpPassword = DecryptString(config.EmailSettings.SmtpPassword);
                        config.LLMSettings.ApiKey = DecryptString(config.LLMSettings.ApiKey);
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Configuration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return new AppConfiguration();
        }

        public static void SaveConfiguration(AppConfiguration config)
        {
            try
            {
                // Create a copy to encrypt sensitive data
                var configToSave = JsonConvert.DeserializeObject<AppConfiguration>(
                    JsonConvert.SerializeObject(config)
                )!;

                // Encrypt sensitive data
                configToSave.EmailSettings.Password = EncryptString(config.EmailSettings.Password);
                configToSave.EmailSettings.SmtpPassword = EncryptString(config.EmailSettings.SmtpPassword);
                configToSave.LLMSettings.ApiKey = EncryptString(config.LLMSettings.ApiKey);

                string directory = Path.GetDirectoryName(ConfigFilePath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(configToSave, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configuration: {ex.Message}", "Configuration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return plainText;
            }
        }

        private static string DecryptString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return encryptedText;
            }
        }
    }
}
