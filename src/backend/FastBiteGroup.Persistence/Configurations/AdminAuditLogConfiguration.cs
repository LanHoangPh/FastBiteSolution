namespace FastBiteGroup.Persistence.Configurations;

internal sealed class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.ToTable(TableNames.AdminAuditLogs);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AdminFullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ActionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.TargetEntityType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(x => x.TargetEntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Details)
            .HasMaxLength(4000);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.HasIndex(x => new { x.AdminUserId, x.Timestamp });
        builder.HasIndex(x => x.BatchId);

        // FK : AdminUserId -> AppUser.Id
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
