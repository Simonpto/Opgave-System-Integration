namespace Shared.Contracts.UserProfiles;

/// <summary>Public view of a user profile. Internal fields (email, activity counters) are deliberately left out.</summary>
public record UserProfileDto(Guid Id, string Username, string DisplayName, string? AvatarUrl, DateTime CreatedAt);
