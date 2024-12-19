public class FileLogger
{
    private readonly string _logDirectory;
    
    public FileLogger()
    {
        _logDirectory = "Logs"; // Default log directory
        Directory.CreateDirectory(_logDirectory); // Ensure the log directory exists
    }

    public async Task LogToFileAsync(string message)
    {
        try
        {
            var logFilePath = Path.Combine(_logDirectory, $"log-{DateTime.UtcNow:yyyy-MM-dd}.txt");

            await using (var writer = new StreamWriter(logFilePath, append: true))
            {
                await writer.WriteLineAsync($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
}