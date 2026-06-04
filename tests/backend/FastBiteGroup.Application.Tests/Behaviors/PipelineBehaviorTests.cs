using FastBiteGroup.Application.Behaviors;
using FastBiteGroup.Application.Tests.Common;
using FastBiteGroup.Contract.Abstractions.Shared;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemException = System.Exception;

namespace FastBiteGroup.Application.Tests.Behaviors;

/// <summary>
/// Test Scenario:
/// - Kiểm tra PerformancePipelineBehavior: log cảnh báo khi request chạy > 5000ms
/// - Quan trọng: behavior phải TRANSPARENT — luôn trả về đúng response
/// - Không nên log warning khi request chạy bình thường
/// </summary>
public class PerformancePipelineBehaviorTests
{
    private readonly ILogger<FakeRequest> _logger;
    private readonly PerformancePipelineBehavior<FakeRequest, Result> _behavior;

    public PerformancePipelineBehaviorTests()
    {
        _logger = Substitute.For<ILogger<FakeRequest>>();
        _behavior = new PerformancePipelineBehavior<FakeRequest, Result>(_logger);
    }

    // =====================================================================
    // Case 1: Request nhanh → không log Warning (happy path)
    // =====================================================================

    [Fact]
    public async Task Handle_WhenRequestIsFastEnough_ShouldNotLogWarning()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(Result.Success()); // response trả về ngay

        var request = new FakeRequest("Test", 100m);

        // Act
        await _behavior.Handle(request, next, CancellationToken.None);

        // Assert — Không log cảnh báo nào (request < 5000ms)
        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<SystemException?>(),
            Arg.Any<Func<object, SystemException?, string>>());
    }

    // =====================================================================
    // Case 2: Behavior phải transparent — luôn trả về response từ next
    // =====================================================================

    [Fact]
    public async Task Handle_Always_ShouldReturnOriginalResponseFromNext()
    {
        // Arrange
        var expectedResult = Result.Success();
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(expectedResult);

        var request = new FakeRequest("Test", 100m);

        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    // =====================================================================
    // Case 3: next phải được gọi đúng 1 lần — behavior không skip
    // =====================================================================

    [Fact]
    public async Task Handle_Always_ShouldCallNextExactlyOnce()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(Result.Success());

        var request = new FakeRequest("Test", 100m);

        // Act
        await _behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await next.Received(1)();
    }

    // =====================================================================
    // Case 4: Failure response cũng phải được trả về đúng
    // =====================================================================

    [Fact]
    public async Task Handle_WhenNextReturnsFailure_ShouldReturnFailureResult()
    {
        // Arrange — Behavior không được "nuốt" failure result
        var failureError = new Error("Handler.Error", "Xử lý thất bại");
        var failureResult = Result.Failure(failureError);

        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(failureResult);

        var request = new FakeRequest("Test", 100m);

        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(failureError);
    }
}

/// <summary>
/// Test Scenario:
/// - Kiểm tra TracingPipelineBehaviors: log thông tin duration cho MỌI request
/// - Behavior phải TRANSPARENT và luôn log Information (khác Performance là chỉ log khi slow)
/// </summary>
public class TracingPipelineBehaviorsTests
{
    private readonly ILogger<FakeRequest> _logger;
    private readonly TracingPipelineBehaviors<FakeRequest, Result> _behavior;

    public TracingPipelineBehaviorsTests()
    {
        _logger = Substitute.For<ILogger<FakeRequest>>();
        _behavior = new TracingPipelineBehaviors<FakeRequest, Result>(_logger);
    }

    // =====================================================================
    // Case 1: Phải log Information với request name (always)
    // =====================================================================

    [Fact]
    public async Task Handle_Always_ShouldLogInformation()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(Result.Success());

        var request = new FakeRequest("Test", 100m);

        // Act
        await _behavior.Handle(request, next, CancellationToken.None);

        // Assert — Phải log Information ít nhất 1 lần
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<SystemException?>(),
            Arg.Any<Func<object, SystemException?, string>>());
    }

    // =====================================================================
    // Case 2: Trả về đúng response — transparent
    // =====================================================================

    [Fact]
    public async Task Handle_Always_ShouldReturnOriginalResponseFromNext()
    {
        // Arrange
        var expectedResult = Result.Success();
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(expectedResult);

        var request = new FakeRequest("Test", 100m);

        // Act
        var result = await _behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }

    // =====================================================================
    // Case 3: next phải được gọi đúng 1 lần
    // =====================================================================

    [Fact]
    public async Task Handle_Always_ShouldCallNextExactlyOnce()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(Result.Success());

        var request = new FakeRequest("Test", 100m);

        // Act
        await _behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await next.Received(1)();
    }

    // =====================================================================
    // Case 4: Log phải chứa tên request (traceability)
    // Dùng CapturingLogger tự viết thay vì NSubstitute Do() callback
    // vì LoggerExtensions wrap Log() call qua generic TState khiến ArgAt cast fail
    // =====================================================================

    [Fact]
    public async Task Handle_Always_ShouldLogWithRequestTypeName()
    {
        // Arrange — Dùng CapturingLogger để bắt messages một cách đáng tin cậy
        var capturingLogger = new CapturingLogger<FakeRequest>();
        var behavior = new TracingPipelineBehaviors<FakeRequest, Result>(capturingLogger);

        var next = Substitute.For<RequestHandlerDelegate<Result>>();
        next().Returns(Result.Success());

        // Act
        await behavior.Handle(new FakeRequest("Test", 100m), next, CancellationToken.None);

        // Assert — Log message phải chứa tên request class
        capturingLogger.Messages.Should().NotBeEmpty();
        capturingLogger.Messages.Should().Contain(msg => msg.Contains("FakeRequest"));
    }
}

/// <summary>
/// Logger implementation thực sự để capture log messages
/// Tránh dùng NSubstitute.ArgAt với generic TState của ILogger vì có vấn đề casting
/// </summary>
internal sealed class CapturingLogger<T> : ILogger<T>
{
    private readonly List<string> _messages = new();
    public IReadOnlyList<string> Messages => _messages;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        System.Exception? exception, Func<TState, System.Exception?, string> formatter)
    {
        _messages.Add(formatter(state, exception));
    }
}

