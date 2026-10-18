using UserProfile.Application.UserProfiles;

namespace UserProfile.Infrastructure.Persistence;

using UserProfile.Domain.Profiles;

/// <summary>
/// Process-local store to get the API running; register as a singleton. Swap for an EF Core implementation later.
/// Usernames are unique case-insensitively.
/// </summary>
public sealed class InMemoryUserProfileRepository : IUserProfileRepository
{
    private readonly object _lock = new();
    private readonly Dictionary<Guid, UserProfile> _byId = new();
    private readonly Dictionary<string, Guid> _idByUsername = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            IReadOnlyList<UserProfile> all = _byId.Values.OrderBy(p => p.CreatedAt).ToList();
            return Task.FromResult(all);
        }
    }

    public Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_byId.GetValueOrDefault(id));
        }
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_idByUsername.ContainsKey(username));
        }
    }

    public Task AddAsync(UserProfile profile, CancellationToken ct = default)
    {
        lock (_lock)
        {
            // Re-checked under the lock so two concurrent creates cannot both claim the same username.
            if (!_idByUsername.TryAdd(profile.Username.Value, profile.Id))
                throw UserProfileService.UsernameTaken(profile.Username.Value);

            _byId.Add(profile.Id, profile);
        }

        return Task.CompletedTask;
    }

    // Entities are held by reference, so changes are already visible; nothing to persist.
    public Task UpdateAsync(UserProfile profile, CancellationToken ct = default) => Task.CompletedTask;

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_byId.Remove(id, out var profile))
                _idByUsername.Remove(profile.Username.Value);
        }

        return Task.CompletedTask;
    }
}
