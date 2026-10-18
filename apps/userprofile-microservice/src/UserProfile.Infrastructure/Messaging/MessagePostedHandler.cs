using MessageClient;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;
using Shared.Contracts.UserProfiles;
using UserProfile.Application.UserProfiles;

namespace UserProfile.Infrastructure.Messaging;

/// <summary>
/// Counts the author's messages and publishes <see cref="UserActivityRecordedEvent"/> (carrying the MessageId for tracing).
/// </summary>
/// <remarks>
/// An unknown author is not an error: the message may be from a user whose profile does not exist (yet). Throwing would
/// only bounce the message to the error queue, so we log, skip the counting and still publish with MessageCount 0.
/// Delivery is at-least-once, so a redelivered message is counted again; dedupe on MessageId if that starts to matter.
/// </remarks>
public sealed class MessagePostedHandler : IMessageHandler<MessagePostedEvent>
{
    private readonly IMessageClient _messageClient;
    private readonly IUserProfileRepository _repository;
    private readonly TimeProvider _time;
    private readonly ILogger<MessagePostedHandler> _logger;

    public MessagePostedHandler(IMessageClient messageClient, IUserProfileRepository repository, TimeProvider time,
                                ILogger<MessagePostedHandler> logger)
    {
        _messageClient = messageClient;
        _repository = repository;
        _time = time;
        _logger = logger;
    }

    public async Task Handle(MessagePostedEvent message, CancellationToken cancellationToken)
    {
        var messageCount = 0;
        var profile = await _repository.GetByIdAsync(message.AuthorId, cancellationToken);

        if (profile is null)
        {
            _logger.LogWarning("Message {MessageId} posted by {AuthorId}, who has no profile; not counted",
                message.MessageId, message.AuthorId);
        }
        else
        {
            profile.RegisterMessagePosted();
            await _repository.UpdateAsync(profile, cancellationToken);
            messageCount = profile.MessageCount;
        }

        await _messageClient.PublishAsync(new UserActivityRecordedEvent
        {
            MessageId = message.MessageId,
            UserId = message.AuthorId,
            MessageCount = messageCount,
            ProcessedAt = _time.GetUtcNow().UtcDateTime
        }, cancellationToken);
    }
}
