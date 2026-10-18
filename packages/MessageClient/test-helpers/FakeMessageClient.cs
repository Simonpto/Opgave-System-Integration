using System.Collections.Concurrent;

namespace MessageClient.TestHelpers;

/// <summary>
/// In-memory <see cref="IMessageClient"/> that records published messages instead of sending them.
/// Use <see cref="DeliverAsync{T}"/> to simulate an incoming message to subscribers.
/// </summary>
public sealed class FakeMessageClient : IMessageClient
{
    private readonly ConcurrentQueue<object> _published = new();
    private readonly ConcurrentDictionary<Type, ConcurrentBag<Func<object, CancellationToken, Task>>> _subscribers = new();

    public IReadOnlyList<object> Published => _published.ToList();

    public IEnumerable<T> PublishedOf<T>() where T : class => _published.OfType<T>();

    /// <summary>Returns the only published message of type <typeparamref name="T"/>; throws if there are zero or several.</summary>
    public T SinglePublished<T>() where T : class
    {
        var matches = PublishedOf<T>().ToList();
        return matches.Count switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException($"Expected one published {typeof(T).Name}, but none were published."),
            _ => throw new InvalidOperationException($"Expected one published {typeof(T).Name}, but {matches.Count} were published.")
        };
    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(message);
        _published.Enqueue(message);
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler,
                                  CancellationToken ct = default) where T : class
    {
        _subscribers.GetOrAdd(typeof(T), _ => new()).Add((message, token) => handler((T)message, token));
        return Task.CompletedTask;
    }

    /// <summary>Invokes every subscriber of <typeparamref name="T"/> with <paramref name="message"/>.</summary>
    public async Task DeliverAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        if (!_subscribers.TryGetValue(typeof(T), out var handlers))
        {
            return;
        }

        foreach (var handler in handlers)
        {
            await handler(message, ct);
        }
    }

    public void Clear() => _published.Clear();
}
