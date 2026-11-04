/// <summary>One domain event waiting to be delivered to other contexts (ADR-005).</summary>
public class OutboxMessage
{
    private OutboxMessage()
    {
        Type = default!;
        Payload = default!;
    }

    public OutboxMessage(string type, string payload, DateTime createdAt)
    {
        Type = type;
        Payload = payload;
        CreatedAt = createdAt;
    }

    public long Id { get; private set; }
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeliveredAt { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
}
