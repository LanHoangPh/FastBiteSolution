using FastBiteGroup.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace FastBiteGroup.Infrastructure.Tests.Services;

/// <summary>
/// Tests for CurrentUserService — verifies correct claim extraction from ClaimsPrincipal.
/// Tests run against the real implementation (not mocked) to ensure claim mapping is correct.
/// </summary>
public class CurrentUserServiceTests
{
    // ─── Helpers ────────────────────────────────────────────────────────────

    private static IHttpContextAccessor BuildAccessor(ClaimsPrincipal? principal = null)
    {
        var context = new DefaultHttpContext
        {
            User = principal ?? new ClaimsPrincipal()
        };

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return accessor;
    }

    /// <summary>
    /// Builds a ClaimsPrincipal exactly as JwtTokenService.GenerateAccessToken would produce.
    /// </summary>
    private static ClaimsPrincipal AuthenticatedPrincipal(
        Guid userId,
        string email = "test@test.com",
        string userName = "testuser",
        string firstName = "First",
        string lastName = "Last",
        string jti = "jti-123",
        string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Name, userName),
            new("firstName", firstName),
            new("lastName", lastName),
            new(JwtRegisteredClaimNames.Jti, jti),
        };

        foreach (var role in roles ?? ["Customer"])
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Use "Bearer" as authentication type so Identity.IsAuthenticated = true
        var identity = new ClaimsIdentity(claims, authenticationType: "Bearer");
        return new ClaimsPrincipal(identity);
    }

    // ─── IsAuthenticated ────────────────────────────────────────────────────

    [Fact]
    public void IsAuthenticated_WhenNoPrincipal_ReturnsFalse()
    {
        var sut = new CurrentUserService(BuildAccessor());
        sut.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_WhenJwtPrincipalPresent_ReturnsTrue()
    {
        var sut = new CurrentUserService(BuildAccessor(AuthenticatedPrincipal(Guid.NewGuid())));
        sut.IsAuthenticated.Should().BeTrue();
    }

    // ─── UserId ─────────────────────────────────────────────────────────────

    [Fact]
    public void UserId_WhenAuthenticated_ReturnsCorrectGuid()
    {
        var expected = Guid.NewGuid();
        var sut = new CurrentUserService(BuildAccessor(AuthenticatedPrincipal(expected)));

        sut.UserId.Should().Be(expected);
    }

    [Fact]
    public void UserId_WhenNotAuthenticated_ReturnsEmptyGuid()
    {
        var sut = new CurrentUserService(BuildAccessor());
        sut.UserId.Should().Be(Guid.Empty);
    }

    // ─── Email ──────────────────────────────────────────────────────────────

    [Fact]
    public void Email_WhenAuthenticated_ReturnsCorrectEmail()
    {
        var sut = new CurrentUserService(BuildAccessor(
            AuthenticatedPrincipal(Guid.NewGuid(), email: "user@example.com")));

        sut.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void Email_WhenNotAuthenticated_ReturnsEmpty()
    {
        var sut = new CurrentUserService(BuildAccessor());
        sut.Email.Should().BeEmpty();
    }

    // ─── Names ──────────────────────────────────────────────────────────────

    [Fact]
    public void FirstName_LastName_WhenAuthenticated_ReturnCorrectValues()
    {
        var sut = new CurrentUserService(BuildAccessor(
            AuthenticatedPrincipal(Guid.NewGuid(), firstName: "Nguyen", lastName: "Van A")));

        sut.FirstName.Should().Be("Nguyen");
        sut.LastName.Should().Be("Van A");
    }

    // ─── JTI ────────────────────────────────────────────────────────────────

    [Fact]
    public void Jti_WhenAuthenticated_ReturnsCorrectJti()
    {
        var sut = new CurrentUserService(BuildAccessor(
            AuthenticatedPrincipal(Guid.NewGuid(), jti: "test-jti-999")));

        sut.Jti.Should().Be("test-jti-999");
    }

    // ─── Roles ──────────────────────────────────────────────────────────────

    [Fact]
    public void Roles_WhenAuthenticated_ReturnsAllRoles()
    {
        var sut = new CurrentUserService(BuildAccessor(
            AuthenticatedPrincipal(Guid.NewGuid(), roles: ["Admin", "Customer"])));

        sut.Roles.Should().BeEquivalentTo(["Admin", "Customer"]);
    }

    [Fact]
    public void Roles_WhenNotAuthenticated_ReturnsEmpty()
    {
        var sut = new CurrentUserService(BuildAccessor());
        sut.Roles.Should().BeEmpty();
    }

    [Fact]
    public void IsInRole_WhenUserHasRole_ReturnsTrue()
    {
        var sut = new CurrentUserService(BuildAccessor(
            AuthenticatedPrincipal(Guid.NewGuid(), roles: ["Admin"])));

        sut.IsInRole("Admin").Should().BeTrue();
        sut.IsInRole("Customer").Should().BeFalse();
    }

    // ─── Null safety ────────────────────────────────────────────────────────

    [Fact]
    public void HttpContextIsNull_HandledGracefully_ReturnsDefaults()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var sut = new CurrentUserService(accessor);

        sut.IsAuthenticated.Should().BeFalse();
        sut.UserId.Should().Be(Guid.Empty);
        sut.Email.Should().BeEmpty();
        sut.UserName.Should().BeEmpty();
        sut.FirstName.Should().BeEmpty();
        sut.LastName.Should().BeEmpty();
        sut.Jti.Should().BeEmpty();
        sut.Roles.Should().BeEmpty();
    }
}
