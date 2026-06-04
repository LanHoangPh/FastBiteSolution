using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using NSubstitute;

namespace FastBiteGroup.Application.Tests.Common.Fixtures;

internal sealed class AuthHandlerFixture
{
    public IUserAuthService UserAuthService { get; } = Substitute.For<IUserAuthService>();
    public IJwtTokenService JwtTokenService { get; } = Substitute.For<IJwtTokenService>();
    public IRefreshTokenRepository RefreshTokenRepository { get; } = Substitute.For<IRefreshTokenRepository>();

    public void GivenUserFoundByEmail(UserDto? user)
    {
        UserAuthService
            .FindByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
    }

    public void GivenUserFoundById(Guid userId, UserDto? user)
    {
        UserAuthService
            .FindByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
    }

    public void GivenUserIsNotLocked(Guid userId)
    {
        UserAuthService
            .IsLockedOutAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);
    }

    public void GivenUserIsLocked(Guid userId)
    {
        UserAuthService
            .IsLockedOutAsync(userId, Arg.Any<CancellationToken>())
            .Returns(true);
    }

    public void GivenPasswordIsValid(Guid userId)
    {
        UserAuthService
            .CheckPasswordAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
    }

    public void GivenPasswordIsInvalid(Guid userId)
    {
        UserAuthService
            .CheckPasswordAsync(userId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    public void GivenTokenPairFor(UserDto user, string accessToken, string jti, DateTime expiresAt, string refreshToken)
    {
        JwtTokenService
            .GenerateAccessToken(user.Id, user.Email, user.UserName, user.FirstName, user.LastName, user.Roles)
            .Returns((accessToken, jti, expiresAt));

        JwtTokenService
            .GenerateRefreshToken()
            .Returns(refreshToken);
    }

    public void GivenRefreshToken(AppRefreshToken? refreshToken)
    {
        RefreshTokenRepository
            .FindByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        if (refreshToken is not null)
        {
            RefreshTokenRepository
                .FindSingleAsync(Arg.Any<System.Linq.Expressions.Expression<Func<AppRefreshToken, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(refreshToken);
        }
    }
}
