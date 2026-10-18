using UserProfile.Domain.Common;

namespace UserProfile.Domain.Profiles;

/// <summary>
/// A user's public profile plus a per-user message counter fed by MessagePostedEvent.
/// "Last active" is deliberately not tracked here: the Presence service owns it.
/// Time is passed in rather than read from the clock so the model stays deterministic and testable.
/// </summary>
public sealed class UserProfile
{
    public const int BioMaxLength = 500;

    public Guid Id { get; private set; }
    public Username Username { get; private set; }
    public DisplayName DisplayName { get; private set; }
    public Email Email { get; private set; }
    public string? Bio { get; private set; }
    public string? AvatarUrl { get; private set; }
    public ProfileStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public int MessageCount { get; private set; }

    private UserProfile(Guid id, Username username, DisplayName displayName, Email email,
                        string? bio, string? avatarUrl, DateTimeOffset now)
    {
        Id = id;
        Username = username;
        DisplayName = displayName;
        Email = email;
        Bio = bio;
        AvatarUrl = avatarUrl;
        Status = ProfileStatus.Active;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public static UserProfile Create(string username, string displayName, string email, DateTimeOffset now,
                                     string? bio = null, string? avatarUrl = null)
    {
        return new UserProfile(
            Guid.NewGuid(),
            new Username(username),
            new DisplayName(displayName),
            new Email(email),
            NormalizeBio(bio),
            NormalizeAvatarUrl(avatarUrl),
            now);
    }

    public void UpdateProfile(string displayName, string? bio, string? avatarUrl, DateTimeOffset now)
    {
        EnsureActive();

        // Validate everything before assigning anything, so a failure leaves the profile untouched.
        var newDisplayName = new DisplayName(displayName);
        var newBio = NormalizeBio(bio);
        var newAvatarUrl = NormalizeAvatarUrl(avatarUrl);

        DisplayName = newDisplayName;
        Bio = newBio;
        AvatarUrl = newAvatarUrl;
        UpdatedAt = now;
    }

    public void ChangeEmail(string email, DateTimeOffset now)
    {
        EnsureActive();

        Email = new Email(email);
        UpdatedAt = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (Status == ProfileStatus.Deactivated)
            return;

        Status = ProfileStatus.Deactivated;
        UpdatedAt = now;
    }

    public void Reactivate(DateTimeOffset now)
    {
        if (Status == ProfileStatus.Active)
            return;

        Status = ProfileStatus.Active;
        UpdatedAt = now;
    }

    /// <summary>
    /// Records that the user posted a message. Activity is not a profile edit, so <see cref="UpdatedAt"/> is untouched.
    /// </summary>
    public void RegisterMessagePosted()
    {
        MessageCount++;
    }

    private void EnsureActive()
    {
        if (Status == ProfileStatus.Deactivated)
            throw new DomainException("A deactivated profile cannot be changed.");
    }

    private static string? NormalizeBio(string? bio)
    {
        var trimmed = bio?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        if (trimmed.Length > BioMaxLength)
            throw new DomainException($"Bio must be at most {BioMaxLength} characters.");
        return trimmed;
    }

    private static string? NormalizeAvatarUrl(string? avatarUrl)
    {
        var trimmed = avatarUrl?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new DomainException("Avatar URL must be an absolute http(s) URL.");
        return trimmed;
    }
}
