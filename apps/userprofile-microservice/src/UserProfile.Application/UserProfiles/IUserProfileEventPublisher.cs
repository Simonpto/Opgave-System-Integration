namespace UserProfile.Application.UserProfiles;

using UserProfile.Domain.Profiles;

/// <summary>Tells other services about profile changes. Publishing is best-effort and must not fail the write.</summary>
public interface IUserProfileEventPublisher
{
    Task PublishCreatedAsync(UserProfile profile, CancellationToken ct = default);
    Task PublishUpdatedAsync(UserProfile profile, CancellationToken ct = default);
    Task PublishDeletedAsync(UserProfile profile, DateTimeOffset deletedAt, CancellationToken ct = default);
}
