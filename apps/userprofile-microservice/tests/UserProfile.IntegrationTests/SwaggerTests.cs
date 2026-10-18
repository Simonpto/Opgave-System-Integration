using System.Net;
using FluentAssertions;

namespace UserProfile.IntegrationTests;

public class SwaggerTests : IClassFixture<UserProfileApiFactory>
{
    private readonly HttpClient _client;

    public SwaggerTests(UserProfileApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_ReturnsOk()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
