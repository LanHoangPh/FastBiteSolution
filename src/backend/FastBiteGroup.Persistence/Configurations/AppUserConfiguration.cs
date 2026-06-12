namespace FastBiteGroup.Persistence.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable(TableNames.AppUser);
        builder.HasKey(user => user.Id);

        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);
        builder.Property(x => x.Bio).HasMaxLength(500);
        builder.Property(x => x.OneSignalPlayerId).HasMaxLength(50);

        builder.Property(x => x.ManualStatus)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);
        builder.Property(x => x.MessagingPrivacy)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);
        builder.HasQueryFilter(x => !x.IsDeleted);

    }
}

