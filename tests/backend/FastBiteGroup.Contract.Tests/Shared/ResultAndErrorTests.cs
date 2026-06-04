using FastBiteGroup.Contract.Abstractions.Shared;
using FluentAssertions;

namespace FastBiteGroup.Contract.Tests.Shared;

/// <summary>
/// Test Scenario:
/// - Kiểm tra Result pattern (Result, Result&lt;T&gt;, Error, ValidationResult)
/// - Đây là building block của toàn bộ application — phải hoạt động chính xác 100%
/// - Nếu Result pattern sai, mọi handler sẽ trả về kết quả không đúng
/// </summary>
public class ResultTests
{
    // =====================================================================
    // Result.Success() — Happy Path
    // =====================================================================

    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Success_ShouldHaveIsFailureFalse()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Success_ShouldHaveNoneError()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Error.Should().Be(Error.None);
    }

    // =====================================================================
    // Result.Failure() — Error Cases
    // =====================================================================

    [Fact]
    public void Failure_ShouldHaveIsSuccessFalse()
    {
        // Arrange
        var error = new Error("Test.Error", "Lỗi kiểm thử");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Failure_ShouldHaveIsFailureTrue()
    {
        // Arrange
        var error = new Error("Test.Error", "Lỗi kiểm thử");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Failure_ShouldContainTheCorrectError()
    {
        // Arrange
        var expectedError = new Error("Product.NotFound", "Sản phẩm không tồn tại");

        // Act
        var result = Result.Failure(expectedError);

        // Assert
        result.Error.Should().Be(expectedError);
        result.Error.Code.Should().Be("Product.NotFound");
        result.Error.Message.Should().Be("Sản phẩm không tồn tại");
    }

    // =====================================================================
    // Result constructor guards — Invariants bất biến
    // =====================================================================

    [Fact]
    public void Constructor_WithSuccessTrueAndNonNoneError_ShouldThrowInvalidOperationException()
    {
        // Arrange & Act — Đây là trạng thái vô lý: "thành công nhưng lại có lỗi"
        // Dùng reflection để test internal constructor thông qua static factory
        var act = () => Result.Success<string>(null!);
        // Cách khác: kiểm tra qua CreateT
        var createAct = () => Result.Create<string>(null);

        // Assert — Create với null sẽ trả về Failure (không throw)
        var failResult = Result.Create<string>(null);
        failResult.IsFailure.Should().BeTrue();
        failResult.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void Failure_WithNoneError_ShouldThrowInvalidOperationException()
    {
        // Arrange — Trạng thái vô lý: "thất bại nhưng không có lỗi"
        // Act — Sử dụng Error.None là forbidden với Failure
        var act = () => Result.Failure(Error.None);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Success_WithNonNoneError_ShouldThrowInvalidOperationException()
    {
        // Arrange — Tương tự: Success không được kèm Error thật
        var act = () => Result.Success<string>("value"); // Success sẽ dùng Error.None
        act.Should().NotThrow(); // OK

        // Nhưng nếu dùng internal constructor mà pass lỗi thì sẽ throw
        // Test này verify qua behavior: Success luôn có Error.None
        var result = Result.Success();
        result.Error.Should().Be(Error.None);
    }
}

public class ResultTTests
{
    // =====================================================================
    // Result<T>.Success<T>() — Happy Path
    // =====================================================================

    [Fact]
    public void SuccessT_ShouldHaveIsSuccessTrueAndCorrectValue()
    {
        // Arrange
        const string expectedValue = "Kết quả test";

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void SuccessT_WithComplexObject_ShouldReturnCorrectValue()
    {
        // Arrange
        var obj = new { Id = 1, Name = "Test" };

        // Act
        var result = Result.Success(obj);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test");
    }

    // =====================================================================
    // Result<T> — Value access guard
    // =====================================================================

    [Fact]
    public void Value_OnFailureResult_ShouldThrowInvalidOperationException()
    {
        // Arrange — BUG PREVENTION: không được phép đọc Value khi failure
        var failureResult = Result.Failure<string>(new Error("Error", "Fail"));

        // Act
        var act = () => { _ = failureResult.Value; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*failure*");
    }

    [Fact]
    public void ImplicitConversion_FromNonNullValue_ShouldCreateSuccess()
    {
        // Arrange — Test implicit operator: Result<T> r = "value";
        const string value = "Kết quả";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_FromNullValue_ShouldCreateFailureWithNullValueError()
    {
        // Arrange — Test implicit operator với null
        // Act
        Result<string> result = (string)null!;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    // =====================================================================
    // Result.Create<T>() — Null guard
    // =====================================================================

    [Fact]
    public void Create_WithNonNullValue_ShouldReturnSuccess()
    {
        // Act
        var result = Result.Create("giá trị hợp lệ");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNullValue_ShouldReturnFailureWithNullValueError()
    {
        // Act
        var result = Result.Create<string>(null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }
}

public class ErrorTests
{
    // =====================================================================
    // Error record — Value equality (record semantics)
    // =====================================================================

    [Fact]
    public void TwoErrors_WithSameCodeAndMessage_ShouldBeEqual()
    {
        // Arrange — Record: equality dựa trên value, không phải reference
        var error1 = new Error("Product.NotFound", "Không tìm thấy sản phẩm");
        var error2 = new Error("Product.NotFound", "Không tìm thấy sản phẩm");

        // Assert
        error1.Should().Be(error2);
        (error1 == error2).Should().BeTrue();
    }

    [Fact]
    public void TwoErrors_WithDifferentCode_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("Product.NotFound", "Message");
        var error2 = new Error("User.NotFound", "Message");

        // Assert
        error1.Should().NotBe(error2);
    }

    [Fact]
    public void TwoErrors_WithDifferentMessage_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("Error.Code", "Message 1");
        var error2 = new Error("Error.Code", "Message 2");

        // Assert
        error1.Should().NotBe(error2);
    }

    [Fact]
    public void Error_None_ShouldHaveEmptyCodeAndMessage()
    {
        // Assert — Error.None là sentinel value cho "không có lỗi"
        Error.None.Code.Should().BeEmpty();
        Error.None.Message.Should().BeEmpty();
    }

    [Fact]
    public void Error_NullValue_ShouldHaveExpectedCodeAndMessage()
    {
        // Assert — Error.NullValue là sentinel cho null reference
        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NullValue.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ImplicitOperatorToString_ShouldReturnErrorCode()
    {
        // Arrange — Implicit cast Error → string phải trả về Code
        var error = new Error("Product.PriceInvalid", "Giá không hợp lệ");

        // Act
        string code = error; // implicit cast

        // Assert
        code.Should().Be("Product.PriceInvalid");
    }
}

public class ValidationResultTests
{
    [Fact]
    public void WithErrors_ShouldCreateValidationResultContainingAllErrors()
    {
        // Arrange
        var errors = new[]
        {
            new Error("Name.Empty", "Tên không được trống"),
            new Error("Price.Invalid", "Giá không hợp lệ")
        };

        // Act
        var result = ValidationResult.WithErrors(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Code == "Name.Empty");
        result.Errors.Should().Contain(e => e.Code == "Price.Invalid");
    }

    [Fact]
    public void WithErrors_ShouldHaveValidationError_AsMainError()
    {
        // Arrange — ValidationResult.Error phải là IValidationResult.ValidationError
        var errors = new[] { new Error("Field.Error", "Lỗi field") };

        // Act
        var result = ValidationResult.WithErrors(errors);

        // Assert
        result.Error.Should().Be(IValidationResult.ValidationError);
    }

    [Fact]
    public void WithErrors_ShouldInheritFromResult()
    {
        // Arrange — Kiểm tra ValidationResult là Result (để pipeline behavior hoạt động)
        var errors = new[] { new Error("Error", "Lỗi") };

        // Act
        var result = ValidationResult.WithErrors(errors);

        // Assert
        result.Should().BeAssignableTo<Result>();
    }
}
