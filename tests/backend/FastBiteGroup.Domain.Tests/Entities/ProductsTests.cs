using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Domain.Exceptions;
using FluentAssertions;
using static FastBiteGroup.Domain.Exceptions.ProductException;

namespace FastBiteGroup.Domain.Tests.Entities;

/// <summary>
/// Test Scenario:
/// - Kiểm tra factory method Products.Create() và method Products.Update()
/// - Đây là Domain Unit Tests: không cần Mock, không cần DB
/// - Bảo vệ business rule: "Giá sản phẩm không được âm"
/// </summary>
public class ProductsTests
{
    // =====================================================================
    // Products.Create() — Happy Path & Edge Cases
    // =====================================================================

    [Fact]
    public void Create_WithValidData_ShouldReturnProductWithCorrectProperties()
    {
        // Arrange
        const string name = "Phở Bò Đặc Biệt";
        const string description = "Phở truyền thống Hà Nội";
        const decimal price = 75_000m;

        // Act
        var product = Products.Create(name, description, price);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldSucceed()
    {
        // Arrange — Boundary: giá = 0 là hợp lệ (ví dụ: món ăn miễn phí, khuyến mãi)
        const decimal zeroPrice = 0m;

        // Act
        var act = () => Products.Create("Bánh mì tặng", "Miễn phí", zeroPrice);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowProductPriceInvalidException()
    {
        // Arrange — Edge Case: giá âm vi phạm business rule
        const decimal negativePrice = -1m;

        // Act
        var act = () => Products.Create("Sản phẩm lỗi", "Mô tả", negativePrice);

        // Assert
        act.Should().Throw<ProductPriceInvalidException>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    [InlineData(-999_999)]
    public void Create_WithAnyNegativePrice_ShouldThrowProductPriceInvalidException(decimal price)
    {
        // Arrange — Parameterized: mọi giá trị âm đều phải bị từ chối
        // Act
        var act = () => Products.Create("Test", "Test", price);

        // Assert
        act.Should().Throw<ProductPriceInvalidException>();
    }

    [Fact]
    public void Create_WithNegativePrice_ExceptionMessage_ShouldContainInvalidPrice()
    {
        // Arrange — Kiểm tra message lỗi rõ ràng để developer debug dễ
        const decimal invalidPrice = -50_000m;

        // Act
        var act = () => Products.Create("Test", "Test", invalidPrice);

        // Assert
        act.Should().Throw<ProductPriceInvalidException>()
           .WithMessage($"*{invalidPrice}*");
    }

    [Fact]
    public void Create_Exception_ShouldInheritFromBadRequestException()
    {
        // Arrange — Kiểm tra hierarchy để GlobalExceptionHandler xử lý đúng HTTP status
        const decimal invalidPrice = -1m;

        // Act
        var act = () => Products.Create("Test", "Test", invalidPrice);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void Create_Exception_ShouldInheritFromDomainException()
    {
        // Arrange — Domain exception hierarchy đảm bảo middleware xử lý đúng
        const decimal invalidPrice = -1m;

        // Act
        var act = () => Products.Create("Test", "Test", invalidPrice);

        // Assert
        act.Should().Throw<DomainException>()
           .Which.Title.Should().Be("Bad Resquest"); // Giữ nguyên typo của source code
    }

    // =====================================================================
    // Products.Update() — Happy Path & Edge Cases
    // =====================================================================

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllProperties()
    {
        // Arrange
        var product = Products.Create("Tên cũ", "Mô tả cũ", 50_000m);
        const string newName = "Tên mới";
        const string newDescription = "Mô tả mới";
        const decimal newPrice = 80_000m;

        // Act
        product.Update(newName, newDescription, newPrice);

        // Assert
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void Update_WithNegativePrice_ShouldThrowProductPriceInvalidException()
    {
        // Arrange — Business rule phải được enforce cả khi update, không chỉ khi create
        var product = Products.Create("Sản phẩm", "Mô tả", 50_000m);

        // Act
        var act = () => product.Update("Tên mới", "Mô tả mới", -1m);

        // Assert
        act.Should().Throw<ProductPriceInvalidException>();
    }

    [Fact]
    public void Update_WithZeroPrice_ShouldSucceed()
    {
        // Arrange — Boundary: giá 0 cũng hợp lệ khi update
        var product = Products.Create("Sản phẩm", "Mô tả", 50_000m);

        // Act
        var act = () => product.Update("Sản phẩm miễn phí", "Khuyến mãi", 0m);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Update_WithNegativePrice_ShouldNotModifyOriginalState()
    {
        // Arrange — Đảm bảo entity không bị mutate một phần khi validation fail
        var product = Products.Create("Tên gốc", "Mô tả gốc", 50_000m);

        // Act
        var act = () => product.Update("Tên mới", "Mô tả mới", -999m);

        // Assert
        act.Should().Throw<ProductPriceInvalidException>();
        // State không thay đổi vì exception throw TRƯỚC khi gán giá trị
        product.Name.Should().Be("Tên gốc");
        product.Price.Should().Be(50_000m);
    }
}
