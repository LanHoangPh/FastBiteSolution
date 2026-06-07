using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Contract.Services.V1.Workspace.Queries;

public sealed record GetWorkspaceByIdQuery(Guid WorkspaceId) : IQuery<WorkspaceDetailResponse>;

public sealed record GetWorkspaceMembersQuery(Guid WorkspaceId) : IQuery<List<WorkspaceMemberResponse>>;

public sealed record GetMyWorkspaceInvitationsQuery() : IQuery<List<WorkspaceInvitationResponse>>;
