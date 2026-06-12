namespace FastBiteGroup.Persistence.Configurations
{
    internal sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable(TableNames.Workspaces);
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("WorkspaceID");

            builder.Property(x => x.WorkspaceName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.Description).HasMaxLength(1000);

            // Cấu hình Enum
            builder.Property(x => x.IsChatEnabled).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.IsFeedEnabled).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.Privacy)
                     .IsRequired()
                     .HasConversion<string>()
                     .HasMaxLength(50);

            // Mối quan hệ: Một User có thể tạo nhiều Workspace
            builder.HasOne<AppUser>()
                   .WithMany()
                   .HasForeignKey(g => g.CreatedByUserID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<AppUser>()
                   .WithMany()
                   .HasForeignKey(g => g.UpdatedByUserID)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

            // Cấu hình query filter cho Soft Delete
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
