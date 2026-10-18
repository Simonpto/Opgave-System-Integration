namespace UserProfile.Presentation.Contracts;

using UserProfile.Domain.Profiles;

public sealed record UserProfileResponse(
    Guid Id,
    string Username,
    string DisplayName,
    string Email,
    string? Bio,
    string? AvatarUrl,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessageCount)
{
    public static UserProfileResponse From(UserProfile profile) => new(
        profile.Id,
        profile.Username.Value,
        profile.DisplayName.Value,
        profile.Email.Value,
        profile.Bio,
        profile.AvatarUrl,
        profile.Status.ToString(),
        profile.CreatedAt.UtcDateTime,
        profile.UpdatedAt.UtcDateTime,
        profile.MessageCount);
}
