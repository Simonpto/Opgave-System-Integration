using System.ComponentModel.DataAnnotations;
using UserProfile.Domain.Profiles;

namespace UserProfile.Presentation.Contracts;

// Annotations catch the obvious shape errors up front; the domain value objects remain the source of truth.
public sealed record CreateUserProfileRequest(
    [Required, StringLength(Username.MaxLength, MinimumLength = Username.MinLength)] string Username,
    [Required, StringLength(DisplayName.MaxLength)] string DisplayName,
    [Required, EmailAddress, StringLength(Email.MaxLength)] string Email,
    [StringLength(Domain.Profiles.UserProfile.BioMaxLength)] string? Bio,
    [Url] string? AvatarUrl);
