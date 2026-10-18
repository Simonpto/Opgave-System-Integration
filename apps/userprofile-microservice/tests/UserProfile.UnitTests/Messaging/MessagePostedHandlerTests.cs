using FluentAssertions;
using MessageClient.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Contracts.Events;
using Shared.Contracts.UserProfiles;
using UserProfile.Infrastructure.Messaging;
using UserProfile.Infrastructure.Persistence;

namespace UserProfile.UnitTests.Messaging;

using UserProfile.Domain.Profiles;

public class MessagePostedHandlerTests
{
    private readonly FakeMessageClient _client = new();
    private readonly InMemoryUserProfileRepository _repository = new();

    private MessagePostedHandler CreateHandler() =>
        new(_client, _repository, TimeProvider.System, NullLogger<MessagePostedHandler>.Instance);

    private static MessagePostedEvent NewEvent(Guid? messageId = null, Guid? authorId = null) => new()
    {
        MessageId = messageId ?? Guid.NewGuid(),
        ChannelId = Guid.NewGuid(),
        AuthorId = authorId ?? Guid.NewGuid(),
        Content = "Hello world",
        PostedAt = DateTime.UtcNow
    };

    private async Task<UserProfile> SeedProfileAsync()
    {
        var profile = UserProfile.Create("alice", "Alice", "alice@example.com", DateTimeOffset.UtcNow);
        await _repository.AddAsync(profile);
        return profile;
    }

    [Fact]
    public async Task Handle_PublishesResultEvent_WithCorrectMessageId()
    {
        var messageId = Guid.NewGuid();
        var handler = CreateHandler();

        await handler.Handle(NewEvent(messageId), CancellationToken.None);

        var published = _client.SinglePublished<UserActivityRecordedEvent>();
        published.Should().NotBeNull();
        published.MessageId.Should().Be(messageId);
    }

    // Consumer contract: the minimum fields this handler depends on. If the messaging team changes the event in a way
    // that breaks us, this fails in CI instead of in production.
    [Fact]
    public async Task Handler_CanConsume_MinimumValidContract()
    {
        var handler = CreateHandler();

        Func<Task> act = () => handler.Handle(new MessagePostedEvent
        {
            MessageId = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        }, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _client.Published.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_IncrementsAuthorsMessageCount_AndPublishesIt()
    {
        var profile = await SeedProfileAsync();
        var handler = CreateHandler();

        await handler.Handle(NewEvent(authorId: profile.Id), CancellationToken.None);
        await handler.Handle(NewEvent(authorId: profile.Id), CancellationToken.None);

        (await _repository.GetByIdAsync(profile.Id))!.MessageCount.Should().Be(2);
        _client.PublishedOf<UserActivityRecordedEvent>().Select(e => e.MessageCount).Should().Equal(1, 2);
        _client.PublishedOf<UserActivityRecordedEvent>().Should().OnlyContain(e => e.UserId == profile.Id);
    }

    [Fact]
    public async Task Handle_PublishesZeroCount_WhenAuthorHasNoProfile()
    {
        var authorId = Guid.NewGuid();
        var handler = CreateHandler();

        await handler.Handle(NewEvent(authorId: authorId), CancellationToken.None);

        var published = _client.SinglePublished<UserActivityRecordedEvent>();
        published.UserId.Should().Be(authorId);
        published.MessageCount.Should().Be(0);
    }
}
