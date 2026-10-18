using MessageClient;
using MessageClient.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UserProfile.IntegrationTests;

/// <summary>Runs the real API with RabbitMQ replaced by an in-memory fake that records published events.</summary>
public class UserProfileApiFactory : WebApplicationFactory<Program>
{
    public FakeMessageClient Messages { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMessageClient>();
            services.AddSingleton<IMessageClient>(Messages);
        });
    }
}
