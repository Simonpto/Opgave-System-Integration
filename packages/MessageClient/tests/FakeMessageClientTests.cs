using FluentAssertions;
using MessageClient.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageClient.Tests;

public class FakeMessageClientTests
{
    [Fact]
    public async Task SinglePublished_ReturnsThePublishedMessage()
    {
        var client = new FakeMessageClient();
        var message = new PingMessage(Guid.NewGuid(), "hello");

        await client.PublishAsync(message);

        client.SinglePublished<PingMessage>().Should().Be(message);
    }

    [Fact]
    public async Task SinglePublished_Throws_WhenMoreThanOne()
    {
        var client = new FakeMessageClient();
        await client.PublishAsync(new PingMessage(Guid.NewGuid(), "a"));
        await client.PublishAsync(new PingMessage(Guid.NewGuid(), "b"));

        var act = () => client.SinglePublished<PingMessage>();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task MessageCapture_ReceivesDeliveredMessage()
    {
        var client = new FakeMessageClient();
        var capture = await MessageCapture<PingMessage>.StartAsync(client);
        var message = new PingMessage(Guid.NewGuid(), "hello");

        await client.DeliverAsync(message);

        (await capture.WaitForMessageAsync(TimeSpan.FromSeconds(1))).Should().Be(message);
    }

    [Fact]
    public async Task MessageCapture_TimesOut_WhenNothingArrives()
    {
        var capture = await MessageCapture<PingMessage>.StartAsync(new FakeMessageClient());

        var act = () => capture.WaitForMessageAsync(TimeSpan.FromMilliseconds(50));

        await act.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task AddMessageHandler_ResolvesHandlerFromNewScopePerMessage()
    {
        var client = new FakeMessageClient();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IMessageClient>(client);
        services.AddMessageHandler<PingMessage, RecordingPingHandler>();
        await using var provider = services.BuildServiceProvider();

        foreach (var hostedService in provider.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(CancellationToken.None);
        }

        var first = new PingMessage(Guid.NewGuid(), "1");
        var second = new PingMessage(Guid.NewGuid(), "2");
        await client.DeliverAsync(first);
        await client.DeliverAsync(second);

        List<(PingMessage Message, RecordingPingHandler Instance)> calls;
        lock (RecordingPingHandler.Calls)
        {
            calls = RecordingPingHandler.Calls.Where(c => c.Message == first || c.Message == second).ToList();
        }

        calls.Select(c => c.Message).Should().Equal(first, second);
        calls[0].Instance.Should().NotBeSameAs(calls[1].Instance);
    }
}
