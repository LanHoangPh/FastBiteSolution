namespace FastBiteGroup.Persistence.Configurations;

internal sealed class GlobalSettingsConfiguration : IEntityTypeConfiguration<GlobalSettings>
{
    public void Configure(EntityTypeBuilder<GlobalSettings> builder)
    {
        builder.ToTable(TableNames.GlobalSettings);

        builder.HasKey(x => x.SettingKey);

        builder.Property(x => x.SettingKey)
            .HasMaxLength(100);

        builder.Property(x => x.SettingValue)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
