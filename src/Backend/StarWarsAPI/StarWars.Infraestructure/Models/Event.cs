namespace StarWars.Infraestucture.Models
{
    public abstract class DomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid Id { get; } = Guid.NewGuid();
    }

    public class QueryExecutedEvent : DomainEvent
    {
        public int QueryId { get; set; }
        public string QueryText { get; set; } = string.Empty;
        public int ExecutionTimeMs { get; set; }
        public bool IsSuccessful { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class StatisticsUpdateEvent : DomainEvent
    {
        public string Trigger { get; set; } = "Scheduled";
    }
}
