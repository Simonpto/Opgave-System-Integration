using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageClient;

public static class ServiceCollectionExtensions
{
    public const string HostConfigKey = "RabbitMQ:Host";
    public const string PortConfigKey = "RabbitMQ:Port";
    public const string UsernameConfigKey = "RabbitMQ:Username";
    public const string PasswordConfigKey = "RabbitMQ:Password";

    /// <summary>
    /// Registers <see cref="IMessageClient"/> backed by RabbitMQ. Reads the <c>RabbitMQ</c> config section
    /// (env vars <c>RabbitMQ__Host</c> etc.): <c>Host</c> defaults to <c>localhost</c>; <c>Port</c>,
    /// <c>Username</c> and <c>Password</c> are optional and fall back to the broker defaults (5672, guest/guest).
    /// </summary>
    public static IServiceCollection AddMessageClient(this IServiceCollection services, IConfiguration configuration)
    {
        var host = configuration[HostConfigKey];
        if (string.IsNullOrWhiteSpace(host))
        {
            host = "localhost";
        }

        var connectionString = $"host={host}";
        if (configuration[PortConfigKey] is { Length: > 0 } port)
            connectionString += $";port={port}";
        if (configuration[UsernameConfigKey] is { Length: > 0 } username)
            connectionString += $";username={username}";
        if (configuration[PasswordConfigKey] is { Length: > 0 } password)
            connectionString += $";password={password}";

        services.AddEasyNetQ(connectionString).UseSystemTextJson();
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
