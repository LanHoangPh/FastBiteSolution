using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace FastBiteGroup.Presentation.APIs;

public class WorkspaceApi : ApiEndpoint, IEndpoint
{
    private const string BaseUrl = "/api/v{version:apiVersion}/workspaces";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.NewVersionedApi("Workspace")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .WithTags("Workspace")
            .RequireAuthorization(); // Protect with Authentication

        // POST: /api/v1/workspaces - Create Workspace
        group.MapPost("/", async ([FromBody] CreateWorkspaceCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }).WithName("CreateWorkspace");

        // GET: /api/v1/workspaces/me - Get My Workspaces
        group.MapGet("/me", async (ISender sender) =>
        {
            var query = new GetMyWorkspacesQuery();
            var result = await sender.Send(query);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }).WithName("GetMyWorkspaces");
    }
}
