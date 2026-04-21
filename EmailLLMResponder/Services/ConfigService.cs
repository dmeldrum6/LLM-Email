using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EmailLLMResponder.Models;
using Newtonsoft.Json;

namespace EmailLLMResponder.Services
{
    public class ConfigService
    {
        private readonly string _configPath;
        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("EmailLLMResponder_Salt_2024");

        public ConfigService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "EmailLLMResponder");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, "config.json");
        }

        public AppConfig LoadConfig()
        {
            if (!File.Exists(_configPath))
            {
                return new AppConfig();
            }

            try
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();

                if (!string.IsNullOrEmpty(config.EmailConfig.Password))
                {
                    config.EmailConfig.Password = DecryptString(config.EmailConfig.Password);
                }

                if (!string.IsNullOrEmpty(config.LLMConfig.ApiKey))
                {
                    config.LLMConfig.ApiKey = DecryptString(config.LLMConfig.ApiKey);
                }

                return config;
            }
            catch
            {
                return new AppConfig();
            }
        }

        public void SaveConfig(AppConfig config)
        {
            var configToSave = new AppConfig
            {
                EmailConfig = new EmailConfig
                {
                    ImapServer = config.EmailConfig.ImapServer,
                    ImapPort = config.EmailConfig.ImapPort,
                    ImapUseSsl = config.EmailConfig.ImapUseSsl,
                    SmtpServer = config.EmailConfig.SmtpServer,
                    SmtpPort = config.EmailConfig.SmtpPort,
                    SmtpUseSsl = config.EmailConfig.SmtpUseSsl,
                    EmailAddress = config.EmailConfig.EmailAddress,
                    Password = !string.IsNullOrEmpty(config.EmailConfig.Password)
                        ? EncryptString(config.EmailConfig.Password)
                        : string.Empty,
                    CheckIntervalSeconds = config.EmailConfig.CheckIntervalSeconds
                },
                LLMConfig = new LLMConfig
                {
                    ApiEndpoint = config.LLMConfig.ApiEndpoint,
                    ApiKey = !string.IsNullOrEmpty(config.LLMConfig.ApiKey)
                        ? EncryptString(config.LLMConfig.ApiKey)
                        : string.Empty,
                    Model = config.LLMConfig.Model,
                    Temperature = config.LLMConfig.Temperature,
                    MaxTokens = config.LLMConfig.MaxTokens,
                    SystemPrompt = config.LLMConfig.SystemPrompt,
                    EnableRefinementLoop = config.LLMConfig.EnableRefinementLoop,
                    RefinementPasses = config.LLMConfig.RefinementPasses
                }
            };

            var json = JsonConvert.SerializeObject(configToSave, Formatting.Indented);
            File.WriteAllText(_configPath, json);
        }

        private string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = ProtectedData.Protect(plainBytes, _entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return plainText;
            }
        }

        private string DecryptString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var plainBytes = ProtectedData.Unprotect(encryptedBytes, _entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return encryptedText;
            }
        }
    }
}
