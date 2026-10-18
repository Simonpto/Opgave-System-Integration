using FluentAssertions;
using UserProfile.Domain.Common;
using UserProfile.Domain.Profiles;

namespace UserProfile.UnitTests.Domain;

public class DisplayNameTests
{
    [Fact]
    public void Create_TrimsWhitespace()
    {
        new DisplayName("  Alice Ærø ").Value.Should().Be("Alice Ærø");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_Throws_WhenEmpty(string? value)
    {
        var act = () => new DisplayName(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenTooLong()
    {
        var act = () => new DisplayName(new string('a', DisplayName.MaxLength + 1));

        act.Should().Throw<DomainException>();
    }
}
