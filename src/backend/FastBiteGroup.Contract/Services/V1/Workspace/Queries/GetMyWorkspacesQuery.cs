using FastBiteGroup.Contract.Abstractions.Message;
using FastBiteGroup.Contract.Services.V1.Workspace.Responses;

namespace FastBiteGroup.Contract.Services.V1.Workspace.Queries;

public record GetMyWorkspacesQuery() : IQuery<List<WorkspaceResponse>>;
