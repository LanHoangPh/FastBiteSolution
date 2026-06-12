namespace FastBiteGroup.Persistence.Configurations;

internal sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable(TableNames.WorkspaceMembers);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("WorkspaceMemberID");

        builder.HasIndex(x => new { x.WorkspaceID, x.UserID }).IsUnique();
        builder.HasQueryFilter(x => x.Workspace != null && !x.Workspace.IsDeleted);

        builder.Property(x => x.Role)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(x => x.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(x => x.JoinedAt).IsRequired();
        builder.Property(x => x.LeftAt).IsRequired(false);

        builder.HasOne(gm => gm.Workspace)
               .WithMany(g => g.Members)
               .HasForeignKey(gm => gm.WorkspaceID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(gm => gm.UserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
