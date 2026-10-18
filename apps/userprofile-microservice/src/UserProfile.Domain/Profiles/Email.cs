using System.Text.RegularExpressions;
using UserProfile.Domain.Common;

namespace UserProfile.Domain.Profiles;

/// <summary>A syntactically plausible address, trimmed and lower-cased so equality is case-insensitive.</summary>
public sealed partial record Email
{
    public const int MaxLength = 254;

    public string Value { get; }

    public Email(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(normalized))
            throw new DomainException("Email is required.");
        if (normalized.Length > MaxLength)
            throw new DomainException($"Email must be at most {MaxLength} characters.");
        if (!Format().IsMatch(normalized))
            throw new DomainException("Email is not a valid address.");

        Value = normalized;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex Format();
}
