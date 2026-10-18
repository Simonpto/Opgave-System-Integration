using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageClient;

public static class ServiceCollectionExtensions
{
    public const string HostConfigKey = "RabbitMQ:Host";

    /// <summary>
    /// Registers <see cref="IMessageClient"/> backed by RabbitMQ. The host is read from <c>RabbitMQ:Host</c>
    /// (env var <c>RabbitMQ__Host</c>) and defaults to <c>localhost</c>.
    /// </summary>
    public static IServiceCollection AddMessageClient(this IServiceCollection services, IConfiguration configuration)
    {
        var host = configuration[HostConfigKey];
        if (string.IsNullOrWhiteSpace(host))
        {
            host = "localhost";
        }

        services.AddEasyNetQ($"host={host}").UseSystemTextJson();
        services.AddSingleton<IMessageClient, EasyNetQMessageClient>();

        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="THandler"/> (scoped) and a hosted service that subscribes it on startup.
    /// Instances of the same service share <paramref name="subscriptionId"/> (default: the handler's full type name)
    /// and so split the work; different services get their own copy of every message.
    /// </summary>
    public static IServiceCollection AddMessageHandler<TMessage, THandler>(
        this IServiceCollection services,
        string? subscriptionId = null)
        where TMessage : class
        where THandler : class, IMessageHandler<TMessage>
    {
        var id = subscriptionId ?? typeof(THandler).FullName!;

        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        services.AddSingleton<IHostedService>(sp => new MessageHandlerSubscriber<TMessage>(
            sp.GetRequiredService<IMessageClient>(),
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<ILogger<MessageHandlerSubscriber<TMessage>>>(),
            id));

        return services;
    }
}
