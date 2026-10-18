using System.ComponentModel.DataAnnotations;
using UserProfile.Domain.Profiles;

namespace UserProfile.Presentation.Contracts;

/// <summary>Full replacement of the editable fields; omitted Bio/AvatarUrl are cleared. Username and email cannot be changed here.</summary>
public sealed record UpdateUserProfileRequest(
    [Required, StringLength(DisplayName.MaxLength)] string DisplayName,
    [StringLength(Domain.Profiles.UserProfile.BioMaxLength)] string? Bio,
    [Url] string? AvatarUrl);
