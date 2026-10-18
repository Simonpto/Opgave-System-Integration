namespace Shared.Contracts.UserProfiles;

public record UserProfileCreatedEvent(Guid Id, string Username, string DisplayName, string? AvatarUrl, DateTime CreatedAt);
