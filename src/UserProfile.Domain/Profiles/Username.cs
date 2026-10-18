using System.Text.RegularExpressions;
using UserProfile.Domain.Common;

namespace UserProfile.Domain.Profiles;

/// <summary>3–30 characters: letters, digits, underscore, dot or hyphen. Surrounding whitespace is trimmed.</summary>
public sealed partial record Username
{
    public const int MinLength = 3;
    public const int MaxLength = 30;

    public string Value { get; }

    public Username(string? value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("Username is required.");
        if (trimmed.Length is < MinLength or > MaxLength)
            throw new DomainException($"Username must be between {MinLength} and {MaxLength} characters.");
        if (!AllowedCharacters().IsMatch(trimmed))
            throw new DomainException("Username may only contain letters, digits, '_', '.' and '-'.");

        Value = trimmed;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Za-z0-9_.-]+$")]
    private static partial Regex AllowedCharacters();
}
