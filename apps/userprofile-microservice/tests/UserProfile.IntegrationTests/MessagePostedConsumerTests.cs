using System.Net.Http.Json;
using FluentAssertions;
using MessageClient.TestHelpers;
using Shared.Contracts.Events;
using Shared.Contracts.UserProfiles;
using UserProfile.Presentation.Contracts;

namespace UserProfile.IntegrationTests;

[Trait("Category", "RabbitMQ")]
public class MessagePostedConsumerTests : IClassFixture<RabbitMqApiFactory>
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly RabbitMqApiFactory _factory;

    public MessagePostedConsumerTests(RabbitMqApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MessagePostedEvent_IsConsumed_AndResultEventPublished()
    {
        var messageId = Guid.NewGuid();
        var capture = await MessageCapture<UserActivityRecordedEvent>.StartAsync(_factory.MessageClient);

        // Publish directly to the broker: no HTTP, tests the message path only
        await _factory.MessageClient.PublishAsync(new MessagePostedEvent
        {
            MessageId = messageId,
            ChannelId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        });

        var result = await capture.WaitForMessageAsync(Timeout);
        result.MessageId.Should().Be(messageId);
    }

    [Fact]
    public async Task MessagePostedEvent_IncrementsAuthorsMessageCount()
    {
        var client = _factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/userprofiles",
            new CreateUserProfileRequest("rabbit_author", "Author", "author@example.com", null, null));
        var profile = (await created.Content.ReadFromJsonAsync<UserProfileResponse>())!;
        var capture = await MessageCapture<UserActivityRecordedEvent>.StartAsync(_factory.MessageClient);

        await _factory.MessageClient.PublishAsync(new MessagePostedEvent
        {
            MessageId = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            AuthorId = profile.Id,
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        });

        var result = await capture.WaitForMessageAsync(Timeout);
        result.UserId.Should().Be(profile.Id);
        result.MessageCount.Should().Be(1);
        (await client.GetFromJsonAsync<UserProfileResponse>($"/api/userprofiles/{profile.Id}"))!
            .MessageCount.Should().Be(1);
    }
}
