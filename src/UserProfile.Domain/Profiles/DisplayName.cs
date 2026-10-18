using UserProfile.Domain.Common;

namespace UserProfile.Domain.Profiles;

/// <summary>1–50 characters after trimming surrounding whitespace.</summary>
public sealed record DisplayName
{
    public const int MaxLength = 50;

    public string Value { get; }

    public DisplayName(string? value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException("Display name is required.");
        if (trimmed.Length > MaxLength)
            throw new DomainException($"Display name must be at most {MaxLength} characters.");

        Value = trimmed;
    }

    public override string ToString() => Value;
}
