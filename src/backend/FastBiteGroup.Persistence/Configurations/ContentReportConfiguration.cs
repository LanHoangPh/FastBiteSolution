namespace FastBiteGroup.Persistence.Configurations
{
    internal sealed class ContentReportsConfiguration : IEntityTypeConfiguration<ContentReport>
    {
        public void Configure(EntityTypeBuilder<ContentReport> builder)
        {
            builder.ToTable("ContentReports");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("ReportedContentID");

            builder.Property(x => x.Reason)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(x => x.ReportedContentType)
                   .HasConversion<string>()
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .IsRequired();

            builder.HasQueryFilter(x => !x.IsDeleted && !x.Workspace.IsDeleted);

            // FK: User báo cáo
            builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(x => x.ReportedByUserID)
               .OnDelete(DeleteBehavior.Restrict);

            // FK: Nhóm chứa nội dung
            builder.HasOne(cr => cr.Workspace)
               .WithMany(g => g.ContentReports)
               .HasForeignKey(cr => cr.WorkspaceID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
