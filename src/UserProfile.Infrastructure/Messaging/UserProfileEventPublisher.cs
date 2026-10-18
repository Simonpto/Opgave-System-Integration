using MessageClient;
using Microsoft.Extensions.Logging;
using Shared.Contracts.UserProfiles;
using UserProfile.Application.UserProfiles;

namespace UserProfile.Infrastructure.Messaging;

using UserProfile.Domain.Profiles;

/// <summary>
/// Maps profile changes to the shared contracts and publishes them. Best-effort: a broker outage is logged and
/// swallowed (after a short timeout) so the HTTP write still succeeds. An outbox would make this reliable.
/// </summary>
public sealed class UserProfileEventPublisher : IUserProfileEventPublisher
{
    private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(5);

    private readonly IMessageClient _client;
    private readonly ILogger<UserProfileEventPublisher> _logger;

    public UserProfileEventPublisher(IMessageClient client, ILogger<UserProfileEventPublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task PublishCreatedAsync(UserProfile profile, CancellationToken ct = default) =>
        PublishAsync(new UserProfileCreatedEvent(profile.Id, profile.Username.Value, profile.DisplayName.Value,
            profile.AvatarUrl, profile.CreatedAt.UtcDateTime), ct);

    public Task PublishUpdatedAsync(UserProfile profile, CancellationToken ct = default) =>
        PublishAsync(new UserProfileUpdatedEvent(profile.Id, profile.Username.Value, profile.DisplayName.Value,
            profile.AvatarUrl, profile.UpdatedAt.UtcDateTime), ct);

    public Task PublishDeletedAsync(UserProfile profile, DateTimeOffset deletedAt, CancellationToken ct = default) =>
        PublishAsync(new UserProfileDeletedEvent(profile.Id, profile.Username.Value, deletedAt.UtcDateTime), ct);

    private async Task PublishAsync<T>(T message, CancellationToken ct) where T : class
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(PublishTimeout);

        try
        {
            await _client.PublishAsync(message, timeout.Token);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Failed to publish {EventType}", typeof(T).Name);
        }
    }
}
