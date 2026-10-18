namespace UserProfile.Application.UserProfiles;

using UserProfile.Domain.Profiles;

public interface IUserProfileRepository
{
    Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken ct = default);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    /// <summary>Adds the profile. Throws <see cref="Common.ConflictException"/> if the username is taken.</summary>
    Task AddAsync(UserProfile profile, CancellationToken ct = default);

    Task UpdateAsync(UserProfile profile, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
