namespace UserProfile.Application.Common;

/// <summary>The request is valid but clashes with existing state, e.g. a username that is already taken.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
