namespace UserProfile.Application.UserProfiles;

using UserProfile.Domain.Profiles;

public interface IUserProfileService
{
    /// <exception cref="Common.ConflictException">The username is already taken.</exception>
    Task<UserProfile> CreateAsync(CreateUserProfileCommand command, CancellationToken ct = default);

    Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken ct = default);

    /// <exception cref="Common.NotFoundException">No profile with that id.</exception>
    Task<UserProfile> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <exception cref="Common.NotFoundException">No profile with that id.</exception>
    Task<UserProfile> UpdateAsync(Guid id, UpdateUserProfileCommand command, CancellationToken ct = default);

    /// <exception cref="Common.NotFoundException">No profile with that id.</exception>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
