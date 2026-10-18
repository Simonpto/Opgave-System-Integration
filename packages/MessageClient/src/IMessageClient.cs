namespace MessageClient;

/// <summary>
/// Transport-agnostic pub/sub client. Nothing outside this package should know which broker sits behind it.
/// </summary>
public interface IMessageClient
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Subscribes <paramref name="handler"/> to messages of type <typeparamref name="T"/>.
    /// Subscribers sharing a <paramref name="subscriptionId"/> compete for messages; different ids each get a copy.
    /// </summary>
    Task SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler,
                           CancellationToken ct = default) where T : class;
}
