using System.Net.Sockets;
using FluentAssertions;
using MessageClient.TestHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageClient.Tests;

/// <summary>
/// Requires a local broker: docker run -p 5672:5672 -p 15672:15672 rabbitmq:3-management
/// Skipped when nothing is listening on RabbitMQ:Host (env var RabbitMQ__Host, default localhost) port 5672.
/// </summary>
[Trait("Category", "RabbitMQ")]
public class RabbitMqIntegrationTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    [SkippableFact]
    public async Task PublishedMessage_IsReceivedBySubscriber()
    {
        SkipUnlessRabbitMqIsReachable();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMessageClient(Configuration);
        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IMessageClient>();

        var capture = await MessageCapture<PingMessage>.StartAsync(client);
        var sent = new PingMessage(Guid.NewGuid(), "hello rabbit");

        await client.PublishAsync(sent);

        var received = await capture.WaitForMessageAsync(Timeout);
        received.Should().Be(sent);
    }

    [SkippableFact]
    public async Task AddMessageHandler_HandlesPublishedMessage()
    {
        SkipUnlessRabbitMqIsReachable();

        var received = new TaskCompletionSource<PingMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(received);
                services.AddMessageClient(Configuration);
                services.AddMessageHandler<PingMessage, ForwardingPingHandler>($"test-{Guid.NewGuid():N}");
            })
            .Build();
        await host.StartAsync();

        var sent = new PingMessage(Guid.NewGuid(), "hello handler");
        await host.Services.GetRequiredService<IMessageClient>().PublishAsync(sent);

        (await received.Task.WaitAsync(Timeout)).Should().Be(sent);
        await host.StopAsync();
    }

    private static void SkipUnlessRabbitMqIsReachable()
    {
        var host = Configuration[ServiceCollectionExtensions.HostConfigKey] ?? "localhost";
        try
        {
            using var tcp = new TcpClient();
            tcp.ConnectAsync(host, 5672).Wait(TimeSpan.FromSeconds(1));
            Skip.IfNot(tcp.Connected, $"RabbitMQ not reachable on {host}:5672");
        }
        catch (Exception ex) when (ex is SocketException or AggregateException)
        {
            Skip.If(true, $"RabbitMQ not reachable on {host}:5672");
        }
    }
}
