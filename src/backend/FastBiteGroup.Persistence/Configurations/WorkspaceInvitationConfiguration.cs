namespace FastBiteGroup.Persistence.Configurations;

internal sealed class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
    {
        builder.ToTable(TableNames.WorkspaceInvitations);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("InvitationID");

        builder.Property(x => x.InvitationCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        builder.HasIndex(x => x.InvitationCode).IsUnique();
        builder.HasQueryFilter(x => x.Workspace != null && !x.Workspace.IsDeleted);

        // Mối quan hệ với Workspace
        builder.HasOne(gi => gi.Workspace)
               .WithMany(g => g.WorkspaceInvitations)
               .HasForeignKey(gi => gi.WorkspaceID)
               .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ với User tạo link
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(gi => gi.CreatedByUserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
