using FluentAssertions;
using UserProfile.Domain.Common;

namespace UserProfile.UnitTests.Domain;

// Imported inside the namespace: at file level, `UserProfile` would bind to the root namespace instead of the entity.
using UserProfile.Domain.Profiles;

public class UserProfileTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = Now.AddHours(1);

    private static UserProfile NewProfile() => UserProfile.Create("alice", "Alice", "alice@example.com", Now);

    [Fact]
    public void Create_SetsAllFields()
    {
        var profile = UserProfile.Create("alice", "Alice A.", "Alice@Example.com", Now,
            bio: "Hello", avatarUrl: "https://cdn.example.com/a.png");

        profile.Id.Should().NotBeEmpty();
        profile.Username.Value.Should().Be("alice");
        profile.DisplayName.Value.Should().Be("Alice A.");
        profile.Email.Value.Should().Be("alice@example.com");
        profile.Bio.Should().Be("Hello");
        profile.AvatarUrl.Should().Be("https://cdn.example.com/a.png");
        profile.Status.Should().Be(ProfileStatus.Active);
        profile.CreatedAt.Should().Be(Now);
        profile.UpdatedAt.Should().Be(Now);
        profile.MessageCount.Should().Be(0);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        NewProfile().Id.Should().NotBe(NewProfile().Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Throws_WhenUsernameIsEmpty(string? username)
    {
        var act = () => UserProfile.Create(username!, "Alice", "alice@example.com", Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenDisplayNameIsEmpty()
    {
        var act = () => UserProfile.Create("alice", " ", "alice@example.com", Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenEmailIsInvalid()
    {
        var act = () => UserProfile.Create("alice", "Alice", "not-an-email", Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_Throws_WhenBioIsTooLong()
    {
        var act = () => UserProfile.Create("alice", "Alice", "alice@example.com", Now,
            bio: new string('x', UserProfile.BioMaxLength + 1));

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("/relative/path.png")]
    [InlineData("ftp://example.com/a.png")]
    public void Create_Throws_WhenAvatarUrlIsInvalid(string avatarUrl)
    {
        var act = () => UserProfile.Create("alice", "Alice", "alice@example.com", Now, avatarUrl: avatarUrl);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_StoresBlankBioAndAvatarAsNull()
    {
        var profile = UserProfile.Create("alice", "Alice", "alice@example.com", Now, bio: "  ", avatarUrl: "");

        profile.Bio.Should().BeNull();
        profile.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public void UpdateProfile_ChangesFieldsAndUpdatedAt()
    {
        var profile = NewProfile();

        profile.UpdateProfile("Alice B.", "New bio", "https://cdn.example.com/b.png", Later);

        profile.DisplayName.Value.Should().Be("Alice B.");
        profile.Bio.Should().Be("New bio");
        profile.AvatarUrl.Should().Be("https://cdn.example.com/b.png");
        profile.UpdatedAt.Should().Be(Later);
        profile.CreatedAt.Should().Be(Now);
    }

    [Fact]
    public void UpdateProfile_Throws_AndLeavesProfileUnchanged_WhenDisplayNameIsInvalid()
    {
        var profile = NewProfile();

        var act = () => profile.UpdateProfile("", "New bio", null, Later);

        act.Should().Throw<DomainException>();
        profile.DisplayName.Value.Should().Be("Alice");
        profile.Bio.Should().BeNull();
        profile.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void UpdateProfile_Throws_AndLeavesProfileUnchanged_WhenAvatarUrlIsInvalid()
    {
        var profile = NewProfile();

        var act = () => profile.UpdateProfile("Alice B.", "New bio", "not a url", Later);

        act.Should().Throw<DomainException>();
        profile.DisplayName.Value.Should().Be("Alice");
        profile.Bio.Should().BeNull();
        profile.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void UpdateProfile_Throws_WhenDeactivated()
    {
        var profile = NewProfile();
        profile.Deactivate(Now);

        var act = () => profile.UpdateProfile("Alice B.", null, null, Later);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeEmail_ChangesEmailAndUpdatedAt()
    {
        var profile = NewProfile();

        profile.ChangeEmail("new@example.com", Later);

        profile.Email.Value.Should().Be("new@example.com");
        profile.UpdatedAt.Should().Be(Later);
    }

    [Fact]
    public void Deactivate_ThenReactivate_TogglesStatus()
    {
        var profile = NewProfile();

        profile.Deactivate(Later);
        profile.Status.Should().Be(ProfileStatus.Deactivated);
        profile.UpdatedAt.Should().Be(Later);

        profile.Reactivate(Later.AddHours(1));
        profile.Status.Should().Be(ProfileStatus.Active);
        profile.UpdatedAt.Should().Be(Later.AddHours(1));
    }

    [Fact]
    public void Deactivate_IsIdempotent()
    {
        var profile = NewProfile();
        profile.Deactivate(Later);

        profile.Deactivate(Later.AddHours(1));

        profile.UpdatedAt.Should().Be(Later);
    }

    [Fact]
    public void RegisterMessagePosted_IncrementsCount_WithoutTouchingUpdatedAt()
    {
        var profile = NewProfile();

        profile.RegisterMessagePosted();
        profile.RegisterMessagePosted();

        profile.MessageCount.Should().Be(2);
        profile.UpdatedAt.Should().Be(Now);
    }
}
