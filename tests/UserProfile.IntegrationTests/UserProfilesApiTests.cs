using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.UserProfiles;
using UserProfile.Presentation.Contracts;

namespace UserProfile.IntegrationTests;

public class UserProfilesApiTests : IClassFixture<UserProfileApiFactory>
{
    private const string BaseUrl = "/api/userprofiles";

    private readonly UserProfileApiFactory _factory;
    private readonly HttpClient _client;

    public UserProfilesApiTests(UserProfileApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // The store is shared by every test in this class, so each test uses its own username.
    private static string UniqueUsername() => $"user_{Guid.NewGuid():N}"[..20];

    private static CreateUserProfileRequest ValidCreateRequest(string? username = null) =>
        new(username ?? UniqueUsername(), "Alice", "Alice@Example.com", "Hello", "https://example.com/a.png");

    private async Task<UserProfileResponse> CreateProfileAsync(string? username = null)
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, ValidCreateRequest(username));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserProfileResponse>())!;
    }

    [Fact]
    public async Task Create_Returns201WithLocationAndBody()
    {
        var request = ValidCreateRequest();

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        body!.Id.Should().NotBeEmpty();
        body.Username.Should().Be(request.Username);
        body.DisplayName.Should().Be("Alice");
        body.Email.Should().Be("alice@example.com");
        body.Bio.Should().Be("Hello");
        body.AvatarUrl.Should().Be("https://example.com/a.png");
        body.Status.Should().Be("Active");
        body.MessageCount.Should().Be(0);
        response.Headers.Location!.AbsolutePath.Should().BeEquivalentTo($"{BaseUrl}/{body.Id}");
    }

    [Fact]
    public async Task Create_PublishesUserProfileCreatedEvent()
    {
        var created = await CreateProfileAsync();

        _factory.Messages.PublishedOf<UserProfileCreatedEvent>().Should().ContainSingle(e => e.Id == created.Id)
            .Which.Username.Should().Be(created.Username);
    }

    [Fact]
    public async Task Create_Returns400_WhenRequiredFieldIsMissing()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { username = UniqueUsername(), displayName = "Alice" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Create_Returns400_WhenDomainRuleIsViolated()
    {
        // Passes the annotations (length is fine) but the Username value object rejects the space.
        var response = await _client.PostAsJsonAsync(BaseUrl, ValidCreateRequest("has space"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Contain("Username");
    }

    [Fact]
    public async Task Create_Returns409_WhenUsernameIsTaken()
    {
        var username = UniqueUsername();
        await CreateProfileAsync(username);

        var response = await _client.PostAsJsonAsync(BaseUrl, ValidCreateRequest(username.ToUpperInvariant()));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAll_Returns200WithCreatedProfiles()
    {
        var first = await CreateProfileAsync();
        var second = await CreateProfileAsync();

        var profiles = await _client.GetFromJsonAsync<List<UserProfileResponse>>(BaseUrl);

        profiles!.Select(p => p.Id).Should().Contain([first.Id, second.Id]);
    }

    [Fact]
    public async Task GetById_Returns200_WhenProfileExists()
    {
        var created = await CreateProfileAsync();

        var response = await _client.GetAsync($"{BaseUrl}/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<UserProfileResponse>()).Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GetById_Returns404_WhenProfileDoesNotExist()
    {
        var response = await _client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Returns200AndPersistsChanges()
    {
        var created = await CreateProfileAsync();

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created.Id}",
            new UpdateUserProfileRequest("Alice B.", "New bio", null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await _client.GetFromJsonAsync<UserProfileResponse>($"{BaseUrl}/{created.Id}");
        fetched!.DisplayName.Should().Be("Alice B.");
        fetched.Bio.Should().Be("New bio");
        fetched.AvatarUrl.Should().BeNull();
        fetched.UpdatedAt.Should().BeOnOrAfter(created.UpdatedAt);
        _factory.Messages.PublishedOf<UserProfileUpdatedEvent>().Should().ContainSingle(e => e.Id == created.Id)
            .Which.DisplayName.Should().Be("Alice B.");
    }

    [Fact]
    public async Task Update_Returns400_WhenRequestIsInvalid()
    {
        var created = await CreateProfileAsync();

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created.Id}",
            new UpdateUserProfileRequest("", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Returns404_WhenProfileDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{Guid.NewGuid()}",
            new UpdateUserProfileRequest("Alice", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Returns204_AndProfileIsGone()
    {
        var created = await CreateProfileAsync();

        var response = await _client.DeleteAsync($"{BaseUrl}/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync($"{BaseUrl}/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        _factory.Messages.PublishedOf<UserProfileDeletedEvent>().Should().ContainSingle(e => e.Id == created.Id);
    }

    [Fact]
    public async Task Delete_FreesTheUsername()
    {
        var username = UniqueUsername();
        var created = await CreateProfileAsync(username);
        await _client.DeleteAsync($"{BaseUrl}/{created.Id}");

        var response = await _client.PostAsJsonAsync(BaseUrl, ValidCreateRequest(username));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Delete_Returns404_WhenProfileDoesNotExist()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
