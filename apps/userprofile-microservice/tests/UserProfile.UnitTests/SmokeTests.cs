using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UserProfile.Application;

namespace UserProfile.UnitTests;

public class SmokeTests
{
    [Fact]
    public void AddApplication_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        services.AddApplication().Should().BeSameAs(services);
    }
}
