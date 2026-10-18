namespace Shared.Contracts.UserProfiles;

/// <summary>Carries the full public state after the change, so consumers can overwrite their copy.</summary>
public record UserProfileUpdatedEvent(Guid Id, string Username, string DisplayName, string? AvatarUrl, DateTime UpdatedAt);
