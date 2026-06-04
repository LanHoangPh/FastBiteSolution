using FastBiteGroup.Contract.Abstractions.Shared;
using FastBiteGroup.Contract.Services.V1.Product.Commands;
using FastBiteGroup.Contract.Services.V1.Product.Queries;
using FastBiteGroup.Contract.Services.V1.Product.Responses;
using FastBiteGroup.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FastBiteGroup.Presentation.APIs;

public class ProductApi : ApiEndpoint, IEndpoint
{
    private const string BaseUrl = "/api/v{version:apiVersion}/products";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.NewVersionedApi("Products")
                       .MapGroup(BaseUrl)
                       .HasApiVersion(1)
                       .WithTags("Products")
                       .RequireAuthorization();

        // GET /api/v1/products
        group.MapGet("/", GetAll)
            .WithSummary("Get all products")
            .Produces<PagedResult<ProductResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/products/{id}
        group.MapGet("/{id:int}", GetById)
            .WithSummary("Get product by ID")
            .Produces<ProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/v1/products
        group.MapPost("/", Create)
            .WithSummary("Create a new product")
            .Produces<int>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // PUT /api/v1/products/{id}
        group.MapPut("/{id:int}", Update)
            .WithSummary("Update an existing product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/products/{id}
        group.MapDelete("/{id:int}", Delete)
            .WithSummary("Delete a product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAll(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ProductQueries.GetAllProductsQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetById(
        int id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ProductQueries.GetProductByIdQuery(id), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateProductCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (result.IsFailure) return HandleFailure(result);

        return Results.Created($"/api/v1/products/{result.Value}", result.Value);
    }

    private static async Task<IResult> Update(
        int id,
        [FromBody] UpdateProductRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price);
        var result = await sender.Send(command, ct);
        return result.IsFailure ? HandleFailure(result) : Results.NoContent();
    }

    private static async Task<IResult> Delete(
        int id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteProductCommand(id), ct);
        return result.IsFailure ? HandleFailure(result) : Results.NoContent();
    }
}
public sealed record UpdateProductRequest(string Name, string Description, decimal Price);
