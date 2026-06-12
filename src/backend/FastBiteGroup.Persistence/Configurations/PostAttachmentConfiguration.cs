
namespace FastBiteGroup.Persistence.Configurations;

public class PostAttachmentConfiguration : IEntityTypeConfiguration<PostAttachment>
{
    public void Configure(EntityTypeBuilder<PostAttachment> builder)
    {
        builder.ToTable(TableNames.PostAttachments);
        builder.HasKey(pa => new { pa.PostID, pa.FileID });

        builder.Property(pa => pa.AttachedAt).IsRequired();
        builder.HasQueryFilter(pa =>
            pa.Post != null &&
            !pa.Post.IsDeleted &&
            pa.SharedFile != null &&
            !pa.SharedFile.IsDeleted);

        builder.HasOne(pa => pa.Post)
               .WithMany(p => p.Attachments) // Giả sử class Posts có ICollection<PostAttachment> Attachments
               .HasForeignKey(pa => pa.PostID)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(pa => pa.SharedFile)
               .WithMany() // Giả sử class SharedFiles không cần tham chiếu ngược
               .HasForeignKey(pa => pa.FileID)
               .OnDelete(DeleteBehavior.Cascade); // Nếu xóa File, các liên kết đính kèm cũng bị xóa
    }
}


