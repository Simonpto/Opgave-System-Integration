namespace Shared.Contracts.Events;

// System-wide contract, copied verbatim from the assignment. Do not change it locally: the namespace, type name and
// assembly name (Shared.Contracts) together form the RabbitMQ exchange name, so every team must use the exact same one.
public class MessagePostedEvent
{
    public Guid MessageId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Content { get; set; }
    public DateTime PostedAt { get; set; }
}
