using System;
using System.IO;

public class FileLogger
{
    private readonly string _logDirectory = "Logs";

    public FileLogger()
    {
        // Ensure the log directory exists
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    public void LogToFile(string message)
    {
        string logFilePath = Path.Combine(_logDirectory, $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt");
        string logEntry = $"{DateTime.UtcNow:O} {message}{Environment.NewLine}";

        lock (logFilePath) // Thread-safety for file writes
        {
            File.AppendAllText(logFilePath, logEntry);
        }
    }
}
