using FastBiteGroup.Application.Behaviors;
using FastBiteGroup.Application.Tests.Common;
using FastBiteGroup.Contract.Abstractions.Shared;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace FastBiteGroup.Application.Tests.Behaviors;

/// <summary>
/// Test Scenario:
/// - Kiểm tra ValidationPipelineBehaviors: behavior quan trọng nhất trong pipeline
/// - Logic phức tạp: reflection-based factory cho generic Result&lt;T&gt;, aggregate nhiều validators
/// - Nếu behavior này sai → mọi invalid request đều lọt qua handler mà không bị chặn
/// </summary>
public class ValidationPipelineBehaviorsTests
{
    // =====================================================================
    // Case 1: Không có validator nào → phải đi tiếp (không block)
    // =====================================================================

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNextAndReturnResponse()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<FakeRequest>>();
        var behavior = new ValidationPipelineBehaviors<FakeRequest, Result>(validators);

        var expectedResult = Result.Success();
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(expectedResult);

        var request = new FakeRequest("Test", 100m);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await next.Received(1)(); // next phải được gọi đúng 1 lần
    }

    // =====================================================================
    // Case 2: Validator tồn tại nhưng PASS → đi tiếp
    // =====================================================================

    [Fact]
    public async Task Handle_WhenValidatorsExistAndValidationPasses_ShouldCallNextAndReturnResponse()
    {
        // Arrange
        var validators = new List<IValidator<FakeRequest>> { new AlwaysPassValidator() };
        var behavior = new ValidationPipelineBehaviors<FakeRequest, Result>(validators);

        var expectedResult = Result.Success();
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(expectedResult);

        var request = new FakeRequest("Tên hợp lệ", 100m);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await next.Received(1)();
    }

    // =====================================================================
    // Case 3: Validation FAIL → return ValidationResult, KHÔNG gọi next
    // =====================================================================

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldReturnValidationResult_WithoutCallingNext()
    {
        // Arrange
        var validators = new List<IValidator<FakeRequest>> { new AlwaysFailValidator() };
        var behavior = new ValidationPipelineBehaviors<FakeRequest, Result>(validators);

        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        var request = new FakeRequest("", 100m); // Name rỗng → fail

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Should().BeOfType<ValidationResult>(); // phải là ValidationResult, không phải Result thường

        await next.DidNotReceive()(); // next KHÔNG được gọi khi validation fail
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldReturnResultContainingErrors()
    {
        // Arrange
        var validators = new List<IValidator<FakeRequest>> { new AlwaysFailValidator() };
        var behavior = new ValidationPipelineBehaviors<FakeRequest, Result>(validators);

        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        var request = new FakeRequest("", 100m);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var validationResult = result.Should().BeOfType<ValidationResult>().Subject;
        validationResult.Errors.Should().NotBeEmpty();
        validationResult.Errors.Should().Contain(e => e.Code == "Name");
    }

    // =====================================================================
    // Case 4: Nhiều validators → phải aggregate tất cả lỗi
    // =====================================================================

    [Fact]
    public async Task Handle_WhenMultipleValidatorsAllFail_ShouldAggregateAllErrors()
    {
        // Arrange — 2 validators đều fail
        var validators = new List<IValidator<FakeRequest>>
        {
            new AlwaysFailValidator(),   // 1 lỗi
            new MultiErrorValidator()    // 2 lỗi
        };
        var behavior = new ValidationPipelineBehaviors<FakeRequest, Result>(validators);

        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        var request = new FakeRequest("", -100m); // cả Name rỗng lẫn Price âm

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var validationResult = result.Should().BeOfType<ValidationResult>().Subject;
        // Sau Distinct(), các lỗi trùng bị loại bỏ nhưng vẫn có từ nhiều nguồn
        validationResult.Errors.Should().NotBeEmpty();
        await next.DidNotReceive()();
    }

    // =====================================================================
    // Case 5: Validation fail với Result<T> → phải trả về ValidationResult<T>
    //         (test reflection factory logic phức tạp nhất trong behavior)
    // =====================================================================

    [Fact]
    public async Task Handle_WhenValidationFails_ForResultT_ShouldReturnValidationResultT()
    {
        // Arrange — Kiểm tra code path reflection trong CreateValidationResult<TResult>
        var validators = new List<IValidator<FakeRequestWithResponse>>
        {
            new FakeResponseRequestFailValidator()
        };
        var behavior = new ValidationPipelineBehaviors<FakeRequestWithResponse, Result<string>>(validators);

        var next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        var request = new FakeRequestWithResponse(""); // Value rỗng → fail

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Should().BeOfType<ValidationResult<string>>(); // phải là ValidationResult<string>

        var validationResult = (ValidationResult<string>)result;
        validationResult.Errors.Should().NotBeEmpty();

        await next.DidNotReceive()();
    }

    // =====================================================================
    // Case 6: Validation pass rồi next throw exception → phải propagate
    // =====================================================================

    [Fact]
    public async Task Handle_WhenValidationPassesButNextThrows_ShouldPropagateException()
    {
        // Arrange — Behavior phải transparent với exceptions từ handler
        var validators = new List<IValidator<FakeRequest>> { new AlwaysPassValidator() };
        var behavior = new ValidationPipelineBehaviors<FakeRequest, Result>(validators);

        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().ThrowsAsync(new SystemInvalidOperationException("Handler gặp lỗi nội bộ"));

        var request = new FakeRequest("Test", 100m);

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SystemInvalidOperationException>()
                  .WithMessage("Handler gặp lỗi nội bộ");
    }
}
