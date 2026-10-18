using FluentAssertions;
using UserProfile.Domain.Common;
using UserProfile.Domain.Profiles;

namespace UserProfile.UnitTests.Domain;

public class EmailTests
{
    [Fact]
    public void Create_NormalizesToTrimmedLowerCase()
    {
        new Email("  Alice@Example.COM ").Value.Should().Be("alice@example.com");
    }

    [Fact]
    public void Equality_IsCaseInsensitive()
    {
        new Email("ALICE@example.com").Should().Be(new Email("alice@EXAMPLE.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_Throws_WhenEmpty(string? value)
    {
        var act = () => new Email(value);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("alice")]
    [InlineData("alice@")]
    [InlineData("@example.com")]
    [InlineData("alice@example")]
    [InlineData("al ice@example.com")]
    [InlineData("alice@@example.com")]
    public void Create_Throws_WhenFormatIsInvalid(string value)
    {
        var act = () => new Email(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenTooLong()
    {
        var act = () => new Email(new string('a', Email.MaxLength) + "@example.com");

        act.Should().Throw<DomainException>();
    }
}
