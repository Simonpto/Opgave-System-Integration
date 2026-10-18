namespace MessageClient.Tests;

public record PingMessage(Guid Id, string Text);

public sealed class RecordingPingHandler : IMessageHandler<PingMessage>
{
    public static readonly List<(PingMessage Message, RecordingPingHandler Instance)> Calls = new();

    public Task Handle(PingMessage message, CancellationToken cancellationToken)
    {
        lock (Calls)
        {
            Calls.Add((message, this));
        }

        return Task.CompletedTask;
    }
}

/// <summary>Forwards to whatever the test wires up, so tests can await delivery.</summary>
public sealed class ForwardingPingHandler : IMessageHandler<PingMessage>
{
    private readonly TaskCompletionSource<PingMessage> _received;

    public ForwardingPingHandler(TaskCompletionSource<PingMessage> received)
    {
        _received = received;
    }

    public Task Handle(PingMessage message, CancellationToken cancellationToken)
    {
        _received.TrySetResult(message);
        return Task.CompletedTask;
    }
}
