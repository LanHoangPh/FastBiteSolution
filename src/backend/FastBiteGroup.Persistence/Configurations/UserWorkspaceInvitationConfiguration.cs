namespace FastBiteGroup.Persistentce.Configurations;

internal sealed class UserWorkspaceInvitationConfiguration : IEntityTypeConfiguration<UserWorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<UserWorkspaceInvitation> builder)
    {
        builder.ToTable(TableNames.UserWorkspaceInvitations);
        builder.HasKey(x => x.InvitationID);

        builder.HasIndex(x => new { x.InvitedEmail, x.WorkspaceID })
               .HasDatabaseName("IX_UserWorkspaceInvitations_InvitedEmail_WorkspaceID");

        builder.HasQueryFilter(x => x.Workspace != null && !x.Workspace.IsDeleted);

        builder.Property(x => x.InvitationID)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.InvitedEmail)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(x => x.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.RespondedAt).IsRequired(false);
        builder.Property(x => x.ExpiresAt).IsRequired(false);

        builder.HasOne(invitation => invitation.Workspace)
               .WithMany(workspace => workspace.DirectUserInvitations)
               .HasForeignKey(invitation => invitation.WorkspaceID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(invitation => invitation.InvitedUserID)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(invitation => invitation.InvitedByUserID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
