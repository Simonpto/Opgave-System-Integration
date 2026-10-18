using System.Threading.Channels;

namespace MessageClient.TestHelpers;

/// <summary>
/// Subscribes to <typeparamref name="T"/> and buffers what arrives so a test can await it.
/// Create it before publishing, or the message may be gone before the subscription exists.
/// </summary>
public sealed class MessageCapture<T> where T : class
{
    private readonly Channel<T> _received = Channel.CreateUnbounded<T>();

    private MessageCapture()
    {
    }

    /// <param name="subscriptionId">Defaults to a unique id, so this capture gets its own copy of every message.</param>
    public static async Task<MessageCapture<T>> StartAsync(IMessageClient client, string? subscriptionId = null,
                                                           CancellationToken ct = default)
    {
        var capture = new MessageCapture<T>();
        await client.SubscribeAsync<T>(
            subscriptionId ?? $"capture-{typeof(T).Name}-{Guid.NewGuid():N}",
            (message, token) => capture._received.Writer.WriteAsync(message, token).AsTask(),
            ct);
        return capture;
    }

    /// <summary>Returns the next captured message, or throws <see cref="TimeoutException"/>.</summary>
    public async Task<T> WaitForMessageAsync(TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            return await _received.Reader.ReadAsync(cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            throw new TimeoutException($"No {typeof(T).Name} received within {timeout}.");
        }
    }
}
