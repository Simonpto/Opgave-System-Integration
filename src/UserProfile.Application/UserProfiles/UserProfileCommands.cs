namespace UserProfile.Application.UserProfiles;

public record CreateUserProfileCommand(string Username, string DisplayName, string Email, string? Bio, string? AvatarUrl);

/// <summary>Replaces the editable fields. Username and email are not changed here.</summary>
public record UpdateUserProfileCommand(string DisplayName, string? Bio, string? AvatarUrl);
