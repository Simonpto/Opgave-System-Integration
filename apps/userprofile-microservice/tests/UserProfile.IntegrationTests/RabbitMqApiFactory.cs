using MessageClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.RabbitMq;

namespace UserProfile.IntegrationTests;

/// <summary>Runs the real API against a throwaway RabbitMQ container (requires Docker).</summary>
public class RabbitMqApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string Credentials = "test";

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3-management")
        .WithUsername(Credentials)
        .WithPassword(Credentials)
        .Build();

    public IMessageClient MessageClient => Services.GetRequiredService<IMessageClient>();

    public Task InitializeAsync() => _rabbitMq.StartAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("RabbitMQ:Host", _rabbitMq.Hostname);
        builder.UseSetting("RabbitMQ:Port", _rabbitMq.GetMappedPublicPort(RabbitMqBuilder.RabbitMqPort).ToString());
        builder.UseSetting("RabbitMQ:Username", Credentials);
        builder.UseSetting("RabbitMQ:Password", Credentials);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _rabbitMq.DisposeAsync();
    }
}
