using FastBiteGroup.Application.Tests.Common.Builders;
using FastBiteGroup.Application.Validators.Products;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using FluentAssertions;

namespace FastBiteGroup.Application.Tests.Validators.Products;

public class ProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _createValidator = new();
    private readonly UpdateProductCommandValidator _updateValidator = new();

    [Fact]
    public void CreateProductCommand_WithValidData_ShouldPass()
    {
        var result = _createValidator.Validate(ProductTestData.CreateCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidCreateCommands))]
    public void CreateProductCommand_WithInvalidData_ShouldFail(CreateProductCommand command, string propertyName)
    {
        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == propertyName);
    }

    [Fact]
    public void UpdateProductCommand_WithValidData_ShouldPass()
    {
        var result = _updateValidator.Validate(ProductTestData.UpdateCommand());

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidUpdateCommands))]
    public void UpdateProductCommand_WithInvalidData_ShouldFail(UpdateProductCommand command, string propertyName)
    {
        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == propertyName);
    }

    public static TheoryData<CreateProductCommand, string> InvalidCreateCommands()
        => new()
        {
            { ProductTestData.CreateCommand(name: ""), nameof(CreateProductCommand.Name) },
            { ProductTestData.CreateCommand(name: new string('A', 201)), nameof(CreateProductCommand.Name) },
            { ProductTestData.CreateCommand(description: ""), nameof(CreateProductCommand.Description) },
            { ProductTestData.CreateCommand(description: new string('A', 2001)), nameof(CreateProductCommand.Description) },
            { ProductTestData.CreateCommand(price: -1), nameof(CreateProductCommand.Price) }
        };

    public static TheoryData<UpdateProductCommand, string> InvalidUpdateCommands()
        => new()
        {
            { ProductTestData.UpdateCommand(id: 0), nameof(UpdateProductCommand.Id) },
            { ProductTestData.UpdateCommand(name: ""), nameof(UpdateProductCommand.Name) },
            { ProductTestData.UpdateCommand(name: new string('A', 201)), nameof(UpdateProductCommand.Name) },
            { ProductTestData.UpdateCommand(description: ""), nameof(UpdateProductCommand.Description) },
            { ProductTestData.UpdateCommand(description: new string('A', 2001)), nameof(UpdateProductCommand.Description) },
            { ProductTestData.UpdateCommand(price: -1), nameof(UpdateProductCommand.Price) }
        };
}
