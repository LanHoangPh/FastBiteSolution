using FastBiteGroup.Contract.Abstractions.Shared;
using FluentAssertions;

namespace FastBiteGroup.Application.Tests.Common.Assertions;

internal static class ResultAssertions
{
    public static T ShouldBeSuccess<T>(this Result<T> result)
    {
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    public static void ShouldBeSuccess(this Result result)
    {
        result.IsSuccess.Should().BeTrue();
    }

    public static Error ShouldFailWith(this Result result, string expectedCode)
    {
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedCode);
        return result.Error;
    }

    public static Error ShouldFailWith<T>(this Result<T> result, string expectedCode)
    {
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedCode);
        return result.Error;
    }
}
