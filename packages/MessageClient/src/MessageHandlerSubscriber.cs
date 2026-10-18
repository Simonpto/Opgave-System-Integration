using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageClient;

/// <summary>
/// Subscribes on startup and dispatches each message to an <see cref="IMessageHandler{T}"/> resolved from a fresh DI scope.
/// </summary>
internal sealed class MessageHandlerSubscriber<TMessage> : BackgroundService where TMessage : class
{
    private readonly IMessageClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessageHandlerSubscriber<TMessage>> _logger;
    private readonly string _subscriptionId;

    public MessageHandlerSubscriber(
        IMessageClient client,
        IServiceScopeFactory scopeFactory,
        ILogger<MessageHandlerSubscriber<TMessage>> logger,
        string subscriptionId)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _subscriptionId = subscriptionId;
    }

    // Subscribe in StartAsync (not ExecuteAsync) so the host has finished subscribing before it reports started;
    // otherwise messages published right after startup could be lost.
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.SubscribeAsync<TMessage>(_subscriptionId, HandleAsync, cancellationToken);
        _logger.LogInformation("Subscribed to {MessageType} as {SubscriptionId}", typeof(TMessage).Name, _subscriptionId);
        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    private async Task HandleAsync(TMessage message, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

        try
        {
            await handler.Handle(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handling {MessageType} failed", typeof(TMessage).Name);
            throw;
        }
    }
}
