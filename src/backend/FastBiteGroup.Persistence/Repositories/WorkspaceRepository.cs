using FastBiteGroup.Domain.Abstractions.Repositories;

namespace FastBiteGroup.Persistence.Repositories;

public class WorkspaceRepository : RepositoryBase<Workspace, Guid>, IWorkspaceRepository
{
    private readonly ApplicationDbContext _context;

    public WorkspaceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<WorkspaceSummary>> GetActiveWorkspacesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .AsNoTracking()
            .Where(w => !w.IsArchived)
            .Where(w => w.Members.Any(m =>
                m.UserID == userId &&
                m.Status == EnumWorkspaceMemberStatus.Active &&
                m.LeftAt == null))
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WorkspaceSummary(
                w.Id,
                w.WorkspaceName,
                w.Description,
                w.IsChatEnabled,
                w.IsFeedEnabled,
                w.Privacy,
                w.WorkspaceAvatarUrl,
                w.Members
                    .Where(m => m.UserID == userId)
                    .Select(m => m.Role)
                    .FirstOrDefault(),
                w.CreatedAt,
                w.IsArchived,
                w.Members.Count(m =>
                    m.Status == EnumWorkspaceMemberStatus.Active &&
                    m.LeftAt == null)))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceSummary?> GetWorkspaceSummaryForMemberAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .AsNoTracking()
            .Where(w => w.Id == workspaceId && !w.IsArchived)
            .Where(w => w.Members.Any(m =>
                m.UserID == userId &&
                m.Status == EnumWorkspaceMemberStatus.Active &&
                m.LeftAt == null))
            .Select(w => new WorkspaceSummary(
                w.Id,
                w.WorkspaceName,
                w.Description,
                w.IsChatEnabled,
                w.IsFeedEnabled,
                w.Privacy,
                w.WorkspaceAvatarUrl,
                w.Members
                    .Where(m => m.UserID == userId)
                    .Select(m => m.Role)
                    .FirstOrDefault(),
                w.CreatedAt,
                w.IsArchived,
                w.Members.Count(m =>
                    m.Status == EnumWorkspaceMemberStatus.Active &&
                    m.LeftAt == null)))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Workspace?> GetWorkspaceForUpdateAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);
    }

    public async Task<WorkspaceMember?> GetMemberForUpdateAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .FirstOrDefaultAsync(m => m.WorkspaceID == workspaceId && m.UserID == userId, cancellationToken);
    }

    public async Task<WorkspaceMember?> GetActiveMemberAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.WorkspaceID == workspaceId &&
                m.UserID == userId &&
                m.Status == EnumWorkspaceMemberStatus.Active &&
                m.LeftAt == null,
                cancellationToken);
    }

    public async Task<List<WorkspaceMemberSummary>> GetActiveMembersAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .Where(m =>
                m.WorkspaceID == workspaceId &&
                m.Workspace != null &&
                !m.Workspace.IsArchived &&
                m.Status == EnumWorkspaceMemberStatus.Active &&
                m.LeftAt == null)
            .Join(
                _context.Users.AsNoTracking(),
                member => member.UserID,
                user => user.Id,
                (member, user) => new WorkspaceMemberSummary(
                    member.Id,
                    member.UserID,
                    user.Email ?? string.Empty,
                    user.FullName,
                    user.AvatarUrl,
                    member.Role,
                    member.Status,
                    member.JoinedAt))
            .OrderBy(m => m.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveOwnersAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceMembers
            .AsNoTracking()
            .CountAsync(m =>
                m.WorkspaceID == workspaceId &&
                m.Role == EnumWorkspaceRole.Owner &&
                m.Status == EnumWorkspaceMemberStatus.Active &&
                m.LeftAt == null,
                cancellationToken);
    }

    public async Task<bool> HasPendingInvitationAsync(
        Guid workspaceId,
        string invitedEmail,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkspaceInvitations
            .AsNoTracking()
            .AnyAsync(i =>
                i.WorkspaceID == workspaceId &&
                i.InvitedEmail == invitedEmail &&
                i.Status == EnumInvitationStatus.Pending &&
                (i.ExpiresAt == null || i.ExpiresAt > DateTimeOffset.UtcNow),
                cancellationToken);
    }

    public async Task<List<WorkspaceInvitationSummary>> GetPendingInvitationsByEmailAsync(
        string invitedEmail,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkspaceInvitations
            .AsNoTracking()
            .Where(i =>
                i.InvitedEmail == invitedEmail &&
                i.Status == EnumInvitationStatus.Pending &&
                (i.ExpiresAt == null || i.ExpiresAt > DateTimeOffset.UtcNow) &&
                i.Workspace != null &&
                !i.Workspace.IsArchived)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new WorkspaceInvitationSummary(
                i.Id,
                i.WorkspaceID,
                i.Workspace!.WorkspaceName,
                i.Workspace.Description,
                i.Workspace.WorkspaceAvatarUrl,
                i.InvitedByUserID,
                i.CreatedAt,
                i.ExpiresAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserWorkspaceInvitation?> GetPendingUserInvitationForUpdateAsync(
        int invitationId,
        string invitedEmail,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserWorkspaceInvitations
            .Include(i => i.Workspace)
            .FirstOrDefaultAsync(i =>
                i.Id == invitationId &&
                i.InvitedEmail == invitedEmail &&
                i.Status == EnumInvitationStatus.Pending,
                cancellationToken);
    }

    public async Task<WorkspaceInvitation?> GetWorkspaceInvitationByCodeForUpdateAsync(
        string invitationCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceInvitations
            .Include(i => i.Workspace)
            .FirstOrDefaultAsync(i => i.InvitationCode == invitationCode, cancellationToken);
    }

    public async Task<bool> InvitationCodeExistsAsync(
        string invitationCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkspaceInvitations
            .AsNoTracking()
            .AnyAsync(i => i.InvitationCode == invitationCode, cancellationToken);
    }

    public void AddMember(WorkspaceMember member)
    {
        _context.WorkspaceMembers.Add(member);
    }

    public void AddUserInvitation(UserWorkspaceInvitation invitation)
    {
        _context.UserWorkspaceInvitations.Add(invitation);
    }

    public void AddWorkspaceInvitation(WorkspaceInvitation invitation)
    {
        _context.WorkspaceInvitations.Add(invitation);
    }
}
