namespace AuthServer.API.Models;

public class LogEntry
{
	public int Id { get; set; } 
	public DateTimeOffset Timestamp { get; set; }
	public string Level { get; set; }
	public string? Exception { get; set; }
	public string MessageTemplate { get; set; }
	public string Properties { get; set; }
	public string Service { get; set; }


	public LogEntry(DateTimeOffset timestamp, string level, string? exception, string messageTemplate, string properties, string service)
	{
		Timestamp = timestamp;
		Level = level;
		Exception = exception;
		MessageTemplate = messageTemplate;
		Properties = properties;
		Service = service;
	}

	public LogEntry()
	{

	}
}
