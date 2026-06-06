using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FastBiteGroup.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<AppUser, AppRoles, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();
    public DbSet<AdminNotifications> AdminNotifications => Set<AdminNotifications>();
    public DbSet<ContentReport> ContentReports => Set<ContentReport>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipants> ConversationParticipants => Set<ConversationParticipants>();
    public DbSet<GlobalSettings> GlobalSettings => Set<GlobalSettings>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupInvitations> GroupInvitations => Set<GroupInvitations>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<PollOptions> PollOptions => Set<PollOptions>();
    public DbSet<Polls> Polls => Set<Polls>();
    public DbSet<PollVotes> PollVotes => Set<PollVotes>();
    public DbSet<PostAttachment> PostAttachments => Set<PostAttachment>();
    public DbSet<PostComments> PostComments => Set<PostComments>();
    public DbSet<PostLikes> PostLikes => Set<PostLikes>();
    public DbSet<Posts> Posts => Set<Posts>();
    public DbSet<AppRefreshToken> RefreshTokens => Set<AppRefreshToken>();
    public DbSet<SharedFiles> SharedFiles => Set<SharedFiles>();
    public DbSet<UserGroupInvitation> UserGroupInvitations => Set<UserGroupInvitation>();
    public DbSet<VideoCallParticipants> VideoCallParticipants => Set<VideoCallParticipants>();
    public DbSet<VideoCallSessions> VideoCallSessions => Set<VideoCallSessions>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
