using MessageClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Events;
using UserProfile.Application.UserProfiles;
using UserProfile.Infrastructure.Messaging;
using UserProfile.Infrastructure.Persistence;

namespace UserProfile.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IUserProfileRepository, InMemoryUserProfileRepository>();

        services.AddMessageClient(configuration);
        services.AddScoped<IUserProfileEventPublisher, UserProfileEventPublisher>();
        services.AddMessageHandler<MessagePostedEvent, MessagePostedHandler>();

        return services;
    }
}
