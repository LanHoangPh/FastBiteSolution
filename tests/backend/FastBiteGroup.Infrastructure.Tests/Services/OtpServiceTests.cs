using FastBiteGroup.Application.Abstractions.Authentication;
using FastBiteGroup.Infrastructure.Services;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace FastBiteGroup.Infrastructure.Tests.Services;

public class OtpServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private readonly Mock<IDatabase> _dbMock;
    private readonly OtpService _otpService;

    public OtpServiceTests()
    {
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
        _dbMock = new Mock<IDatabase>();

        _connectionMultiplexerMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_dbMock.Object);

        _otpService = new OtpService(_connectionMultiplexerMock.Object);
    }

    [Fact]
    public async Task GenerateOtpAsync_Success_ShouldGenerateOtpAndCallRedis()
    {
        // Arrange
        var purpose = "LOGIN";
        var identifier = "test@example.com";
        var expiry = TimeSpan.FromMinutes(5);

        _dbMock.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
            
        _dbMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _otpService.GenerateOtpAsync(purpose, identifier, expiry);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Length.Should().Be(6);

        _dbMock.Verify(x => x.StringSetAsync(
            It.Is<RedisKey>(k => k == $"otp:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}"),
            It.Is<RedisValue>(v => v == result),
            It.Is<TimeSpan?>(t => t == expiry),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), Times.Once);

        _dbMock.Verify(x => x.KeyDeleteAsync(
            It.Is<RedisKey>(k => k == $"otp_attempts:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}"),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenExpiredOrNotFound_ShouldReturnExpiredOrNotFound()
    {
        // Arrange
        var purpose = "LOGIN";
        var identifier = "test@example.com";
        var code = "123456";

        _dbMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _otpService.ValidateOtpAsync(purpose, identifier, code);

        // Assert
        result.Should().Be(OtpValidationResult.ExpiredOrNotFound);
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenMaxAttemptsReached_ShouldInvalidateAndReturnMaxAttemptsReached()
    {
        // Arrange
        var purpose = "LOGIN";
        var identifier = "test@example.com";
        var code = "123456";
        var maxAttempts = 5;

        _dbMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue("654321"));

        _dbMock.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(6);

        _dbMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(2);

        // Act
        var result = await _otpService.ValidateOtpAsync(purpose, identifier, code, maxAttempts);

        // Assert
        result.Should().Be(OtpValidationResult.MaxAttemptsReached);

        _dbMock.Verify(x => x.KeyDeleteAsync(
            It.Is<RedisKey[]>(keys => keys.Length == 2 && 
                                      keys[0] == $"otp:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}" && 
                                      keys[1] == $"otp_attempts:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}"), 
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task ValidateOtpAsync_WhenValidCode_ShouldInvalidateAndReturnSuccess()
    {
        // Arrange
        var purpose = "LOGIN";
        var identifier = "test@example.com";
        var code = "123456";

        _dbMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(code));

        _dbMock.Setup(x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(2);

        _dbMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(2);

        // Act
        var result = await _otpService.ValidateOtpAsync(purpose, identifier, code);

        // Assert
        result.Should().Be(OtpValidationResult.Success);

        _dbMock.Verify(x => x.KeyDeleteAsync(
            It.Is<RedisKey[]>(keys => keys.Length == 2 && 
                                      keys[0] == $"otp:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}" && 
                                      keys[1] == $"otp_attempts:{purpose.ToLowerInvariant()}:{identifier.ToLowerInvariant()}"), 
            It.IsAny<CommandFlags>()), Times.Once);
    }
}
