namespace StarWars.Infraestructure.Models;

public class QueueMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public DateTimeOffset? ProcessAfter { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}