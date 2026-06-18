using System;
using System.IO;

namespace Exceptions
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "error.log");

        public static void LogError(string message, Exception ex = null, string context = "")
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR";
                if (!string.IsNullOrEmpty(context))
                    logEntry += $" [{context}]";

                logEntry += $": {message}";

                if (ex != null)
                {
                    logEntry += $"\nException: {ex.Message}";
                    logEntry += $"\nStackTrace: {ex.StackTrace}";

                    if (ex.InnerException != null)
                    {
                        logEntry += $"\nInnerException: {ex.InnerException.Message}";
                    }
                }

                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine + Environment.NewLine);
            }
            catch { /* Игнорируем ошибки логирования */ }
        }

        public static void LogInfo(string message, string context = "")
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO";
                if (!string.IsNullOrEmpty(context))
                    logEntry += $" [{context}]";
                logEntry += $": {message}";

                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch { }
        }
    }
}