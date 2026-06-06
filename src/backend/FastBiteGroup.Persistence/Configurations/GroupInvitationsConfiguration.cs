namespace FastBiteGroupMCA.Persistentce.Configurations;

internal sealed class GroupInvitationsConfiguration : IEntityTypeConfiguration<GroupInvitations>
{
    public void Configure(EntityTypeBuilder<GroupInvitations> builder)
    {
        builder.ToTable(TableNames.GroupInvitations);
        builder.HasKey(x => x.InvitationID);

        builder.Property(x => x.InvitationCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        builder.HasIndex(x => x.InvitationCode).IsUnique();

        // Mối quan hệ với Group
        builder.HasOne(gi => gi.Group)
               .WithMany(g => g.GroupInvitations)
               .HasForeignKey(gi => gi.GroupID)
               .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ với User tạo link
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(gi => gi.CreatedByUserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}


