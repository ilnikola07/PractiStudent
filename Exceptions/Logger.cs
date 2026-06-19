using System;
using System.IO;

namespace Exceptions
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
        public static void LogError(string message, Exception ex = null, string context = "")
        {
            string logEntry = FormatLogEntry("ERROR", message, context);

            if (ex != null)
            {
                logEntry += $"\nException: {ex.Message}";
                logEntry += $"\nStackTrace: {ex.StackTrace}";

                if (ex.InnerException != null)
                {
                    logEntry += $"\nInnerException: {ex.InnerException.Message}";
                }
            }

            WriteLog(logEntry);
        }
        private static string FormatLogEntry(string level, string message, string context)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}";
            if (!string.IsNullOrEmpty(context))
                entry += $" [{context}]";
            return entry + $": {message}";
        }
        private static void WriteLog(string logEntry)
        {
            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch { /* Игнорируем ошибки логирования */ }
        }
    }
}