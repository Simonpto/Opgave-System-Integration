using UserProfile.Application.Common;

namespace UserProfile.Application.UserProfiles;

using UserProfile.Domain.Profiles;

public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;
    private readonly IUserProfileEventPublisher _events;
    private readonly TimeProvider _time;

    public UserProfileService(IUserProfileRepository repository, IUserProfileEventPublisher events, TimeProvider time)
    {
        _repository = repository;
        _events = events;
        _time = time;
    }

    public async Task<UserProfile> CreateAsync(CreateUserProfileCommand command, CancellationToken ct = default)
    {
        // Domain validation first, so an invalid username is reported as such rather than as a conflict.
        var profile = UserProfile.Create(command.Username, command.DisplayName, command.Email, _time.GetUtcNow(),
            command.Bio, command.AvatarUrl);

        if (await _repository.UsernameExistsAsync(profile.Username.Value, ct))
            throw UsernameTaken(profile.Username.Value);

        await _repository.AddAsync(profile, ct);
        await _events.PublishCreatedAsync(profile, ct);
        return profile;
    }

    public Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken ct = default)
    {
        return _repository.GetAllAsync(ct);
    }

    public async Task<UserProfile> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _repository.GetByIdAsync(id, ct) ?? throw NotFound(id);
    }

    public async Task<UserProfile> UpdateAsync(Guid id, UpdateUserProfileCommand command, CancellationToken ct = default)
    {
        var profile = await GetByIdAsync(id, ct);

        profile.UpdateProfile(command.DisplayName, command.Bio, command.AvatarUrl, _time.GetUtcNow());

        await _repository.UpdateAsync(profile, ct);
        await _events.PublishUpdatedAsync(profile, ct);
        return profile;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var profile = await GetByIdAsync(id, ct);

        await _repository.DeleteAsync(id, ct);
        await _events.PublishDeletedAsync(profile, _time.GetUtcNow(), ct);
    }

    public static ConflictException UsernameTaken(string username) => new($"Username '{username}' is already taken.");

    private static NotFoundException NotFound(Guid id) => new($"User profile '{id}' was not found.");
}
