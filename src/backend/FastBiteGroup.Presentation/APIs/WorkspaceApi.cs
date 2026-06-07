using FastBiteGroup.Contract.Services.V1.Workspace.Commands;
using FastBiteGroup.Contract.Services.V1.Workspace.Queries;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;
using FastBiteGroup.Presentation.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
            .RequireAuthorization();

        group.MapPost("/", CreateWorkspace)
            .WithName("CreateWorkspace")
            .WithSummary("Create a new workspace")
            .Produces<WorkspaceResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/me", GetMyWorkspaces)
            .WithName("GetMyWorkspaces")
            .WithSummary("Get all workspaces the current user is a member of")
            .Produces<List<WorkspaceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/invitations/me", GetMyWorkspaceInvitations)
            .WithName("GetMyWorkspaceInvitations")
            .WithSummary("Get all pending workspace invitations for the current user")
            .Produces<List<WorkspaceInvitationResponse>>(StatusCodes.Status200OK);

        group.MapPost("/invitations/{invitationId:int}/accept", AcceptWorkspaceInvitation)
            .WithName("AcceptWorkspaceInvitation")
            .Produces<WorkspaceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/join", JoinWorkspace)
            .WithName("JoinWorkspace")
            .Produces<WorkspaceResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{workspaceId:guid}", GetWorkspaceById)
            .WithName("GetWorkspaceById")
            .Produces<WorkspaceDetailResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{workspaceId:guid}/members", GetWorkspaceMembers)
            .WithName("GetWorkspaceMembers")
            .Produces<List<WorkspaceMemberResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPatch("/{workspaceId:guid}", UpdateWorkspace)
            .WithName("UpdateWorkspace")
            .Produces<WorkspaceResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{workspaceId:guid}", ArchiveWorkspace)
            .WithName("ArchiveWorkspace")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/{workspaceId:guid}/invitations", InviteWorkspaceMember)
            .WithName("InviteWorkspaceMember")
            .Produces<WorkspaceInvitationResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/{workspaceId:guid}/invite-links", CreateWorkspaceInviteLink)
            .WithName("CreateWorkspaceInviteLink")
            .Produces<WorkspaceInviteLinkResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }


    #region Handler
    private static async Task<IResult> CreateWorkspace(
        [FromBody] CreateWorkspaceCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsFailure
            ? HandleFailure(result)
            : Results.Created($"/api/v1/workspaces/{result.Value.WorkspaceId}", result.Value);
    }

    private static async Task<IResult> GetMyWorkspaces(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetMyWorkspacesQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetMyWorkspaceInvitations(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetMyWorkspaceInvitationsQuery(), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> AcceptWorkspaceInvitation(
        int invitationId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new AcceptWorkspaceInvitationCommand(invitationId), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> JoinWorkspace(
        [FromBody] JoinWorkspaceRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new JoinWorkspaceCommand(request.InvitationCode), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetWorkspaceById(
        Guid workspaceId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkspaceByIdQuery(workspaceId), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetWorkspaceMembers(
        Guid workspaceId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkspaceMembersQuery(workspaceId), ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateWorkspace(
        Guid workspaceId,
        [FromBody] UpdateWorkspaceRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateWorkspaceCommand(
            workspaceId,
            request.WorkspaceName,
            request.Description,
            request.WorkspaceType,
            request.Privacy,
            request.WorkspaceAvatarUrl);

        var result = await sender.Send(command, ct);
        return result.IsFailure ? HandleFailure(result) : Results.Ok(result.Value);
    }

    private static async Task<IResult> ArchiveWorkspace(
        Guid workspaceId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ArchiveWorkspaceCommand(workspaceId), ct);
        return result.IsFailure ? HandleFailure(result) : Results.NoContent();
    }

    private static async Task<IResult> InviteWorkspaceMember(
        Guid workspaceId,
        [FromBody] InviteWorkspaceMemberRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new InviteWorkspaceMemberCommand(workspaceId, request.Email), ct);
        return result.IsFailure
            ? HandleFailure(result)
            : Results.Created($"/api/v1/workspaces/invitations/{result.Value.InvitationId}", result.Value);
    }

    private static async Task<IResult> CreateWorkspaceInviteLink(
        Guid workspaceId,
        [FromBody] CreateWorkspaceInviteLinkRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateWorkspaceInviteLinkCommand(workspaceId, request.ExpiresAt, request.MaxUses),
            ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Results.Created($"/api/v1/workspaces/{workspaceId}/invite-links/{result.Value.InvitationId}", result.Value);
    }

    private sealed record UpdateWorkspaceRequest(
        string WorkspaceName,
        string? Description,
        int WorkspaceType,
        int Privacy,
        string? WorkspaceAvatarUrl);

    private sealed record InviteWorkspaceMemberRequest(string Email);

    private sealed record JoinWorkspaceRequest(string InvitationCode);

    private sealed record CreateWorkspaceInviteLinkRequest(DateTimeOffset? ExpiresAt, int? MaxUses);

    #endregion
}
