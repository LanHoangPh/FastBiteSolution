using FastBiteGroup.Domain.Abstractions.Repositories;
using FastBiteGroup.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FastBiteGroup.Persistence.Repositories;

public class WorkspaceRepository : RepositoryBase<Workspace, Guid>, IWorkspaceRepository
{
    private readonly ApplicationDbContext _context;

    public WorkspaceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Workspace>> GetWorkspacesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .AsNoTracking()
            .Include(w => w.Members)
            .Where(w => w.Members.Any(m => m.UserID == userId && m.LeftAt == null))
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
