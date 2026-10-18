using EasyNetQ;

namespace MessageClient;

public sealed class EasyNetQMessageClient : IMessageClient
{
    private readonly IBus _bus;

    public EasyNetQMessageClient(IBus bus)
    {
        _bus = bus;
    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        return _bus.PubSub.PublishAsync(message, ct);
    }

    public Task SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler,
                                  CancellationToken ct = default) where T : class
    {
        // The subscription lives as long as the bus (a singleton), so the returned handle is intentionally not kept.
        return _bus.PubSub.SubscribeAsync(subscriptionId, handler, _ => { }, ct);
    }
}
