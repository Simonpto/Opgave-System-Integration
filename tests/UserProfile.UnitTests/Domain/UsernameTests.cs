using FluentAssertions;
using UserProfile.Domain.Common;
using UserProfile.Domain.Profiles;

namespace UserProfile.UnitTests.Domain;

public class UsernameTests
{
    [Theory]
    [InlineData("bob")]
    [InlineData("alice_92")]
    [InlineData("first.last-name")]
    public void Create_Accepts_ValidUsername(string value)
    {
        new Username(value).Value.Should().Be(value);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        new Username("  alice ").Value.Should().Be("alice");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_Throws_WhenEmpty(string? value)
    {
        var act = () => new Username(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenTooShort()
    {
        var act = () => new Username(new string('a', Username.MinLength - 1));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenTooLong()
    {
        var act = () => new Username(new string('a', Username.MaxLength + 1));

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("has space")]
    [InlineData("emoji😀")]
    [InlineData("semi;colon")]
    [InlineData("ælle")]
    public void Create_Throws_WhenContainsDisallowedCharacters(string value)
    {
        var act = () => new Username(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equality_IsByValue()
    {
        new Username("alice").Should().Be(new Username("alice"));
    }
}
