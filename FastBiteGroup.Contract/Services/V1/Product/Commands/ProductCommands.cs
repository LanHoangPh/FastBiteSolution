using FastBiteGroup.Contract.Abstractions.Message;

namespace FastBiteGroup.Contract.Services.V1.Product.Commands;

public record CreateProductCommand(string Name, string Description, decimal Price) : ICommand<int>;

public record UpdateProductCommand(int Id, string Name, string Description, decimal Price) : ICommand;

public record DeleteProductCommand(int Id) : ICommand;
