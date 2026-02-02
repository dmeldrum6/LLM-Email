namespace EmailLLMGateway.Services
{
    public class LoggingService
    {
        private static readonly string LogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EmailLLMGateway",
            "logs",
            $"log_{DateTime.Now:yyyy-MM-dd}.txt"
        );

        public event EventHandler<string>? LogEntryAdded;

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logEntry = $"[{timestamp}] [{level}] {message}";

            // Write to file
            try
            {
                string? directory = Path.GetDirectoryName(LogFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore file write errors
            }

            // Raise event for UI
            LogEntryAdded?.Invoke(this, logEntry);
        }

        public void LogError(string message, Exception? ex = null)
        {
            string fullMessage = ex != null ? $"{message}: {ex.Message}" : message;
            Log(fullMessage, LogLevel.Error);
        }

        public void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public void LogInfo(string message)
        {
            Log(message, LogLevel.Info);
        }

        public List<string> GetRecentLogs(int count = 100)
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    var lines = File.ReadAllLines(LogFilePath);
                    return lines.TakeLast(count).ToList();
                }
            }
            catch
            {
                // Ignore errors
            }

            return new List<string>();
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
