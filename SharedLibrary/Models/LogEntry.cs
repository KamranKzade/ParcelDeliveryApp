namespace SharedLibrary.Models;

public class LogEntry
{
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Level { get; set; }
    public string? Exception { get; set; }
    public string MessageTemplate { get; set; }
    public string Properties { get; set; }

    public LogEntry()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    public LogEntry(string level, string? exception, string messageTemplate, string properties)
    {
        Level = level;
        Exception = exception;
        MessageTemplate = messageTemplate;
        Properties = properties;
    }
}
