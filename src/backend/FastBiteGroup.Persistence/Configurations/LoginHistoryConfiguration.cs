namespace FastBiteGroup.Persistence.Configurations;

internal sealed class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable(TableNames.LoginHistories);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LoginTimestamp)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.Property(x => x.WasSuccessful)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.LoginTimestamp });

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
