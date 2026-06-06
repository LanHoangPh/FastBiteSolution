namespace FastBiteGroupMCA.Persistentce.Configurations;

internal sealed class PostsConfiguration : IEntityTypeConfiguration<Posts>
{
    public void Configure(EntityTypeBuilder<Posts> builder)
    {
        builder.ToTable(TableNames.Posts);
        builder.HasKey(x => x.PostID);

        builder.Property(x => x.Title).HasMaxLength(500);
        builder.Property(x => x.ContentJson).IsRequired();
        builder.Property(x => x.ContentHtml).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Cấu hình query filter cho Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Index được lọc tối ưu cho việc lấy bài đăng trong group
        builder.HasIndex(x => new { x.GroupID, x.CreatedAt })
               .HasFilter("[IsDeleted] = 0");

        // Mối quan hệ với tác giả (Author)
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(p => p.AuthorUserID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(p => p.UpdatedByUserID)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        // Mối quan hệ với Group
        builder.HasOne(p => p.Group)
               .WithMany(g => g.Posts)
               .HasForeignKey(p => p.GroupID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}


