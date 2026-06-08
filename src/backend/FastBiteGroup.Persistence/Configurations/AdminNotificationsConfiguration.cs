namespace FastBiteGroup.Persistence.Configurations;

internal sealed class AdminNotificationsConfiguration : IEntityTypeConfiguration<AdminNotifications>
{
    public void Configure(EntityTypeBuilder<AdminNotifications> builder)
    {
        builder.ToTable(TableNames.AdminNotifications);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NotificationType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.LinkTo)
            .HasMaxLength(2048);

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReadAt)
            .IsRequired(false);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.HasIndex(x => new { x.IsRead, x.Timestamp });
        builder.HasIndex(x => x.TriggeredByUserId);

        // FK to AppUser
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.TriggeredByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
