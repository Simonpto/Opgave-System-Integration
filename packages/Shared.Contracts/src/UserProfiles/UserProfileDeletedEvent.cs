namespace Shared.Contracts.UserProfiles;

public record UserProfileDeletedEvent(Guid Id, string Username, DateTime DeletedAt);
