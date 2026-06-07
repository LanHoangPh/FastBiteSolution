using FastBiteGroup.Domain.Entities;

namespace FastBiteGroup.Domain.Abstractions.Repositories;

public interface IWorkspaceRepository : IRepositoryBase<Workspace, Guid>
{
    Task<List<Workspace>> GetWorkspacesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
