namespace UserProfile.Domain.Common;

/// <summary>Thrown when input violates a domain rule.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
