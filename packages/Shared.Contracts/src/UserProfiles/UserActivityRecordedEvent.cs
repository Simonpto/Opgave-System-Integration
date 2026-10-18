namespace Shared.Contracts.UserProfiles;

/// <summary>
/// Published by the UserProfile service after it has processed a <see cref="Events.MessagePostedEvent"/>.
/// <see cref="MessageCount"/> is the author's total after this message; 0 means the author has no profile, so nothing was counted.
/// </summary>
public record UserActivityRecordedEvent
{
    public Guid MessageId { get; init; }
    public Guid UserId { get; init; }
    public int MessageCount { get; init; }
    public DateTime ProcessedAt { get; init; }
}
