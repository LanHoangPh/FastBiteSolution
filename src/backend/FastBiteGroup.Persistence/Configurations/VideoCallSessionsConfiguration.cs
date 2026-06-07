namespace FastBiteGroupMCA.Persistentce.Configurations;

internal sealed class VideoCallSessionsConfiguration : IEntityTypeConfiguration<VideoCallSessions>
{
    public void Configure(EntityTypeBuilder<VideoCallSessions> builder)
    {
        builder.ToTable(TableNames.VideoCallSessions);

        builder.HasKey(x => x.VideoCallSessionID);
        builder.Property(x => x.VideoCallSessionID).ValueGeneratedNever();

        builder.Property(x => x.Title).HasMaxLength(255);

        builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .IsRequired();
        builder.Property(x => x.TimeoutJobId).HasMaxLength(100);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.DeletedAt).IsRequired(false);

        builder.HasQueryFilter(x => !x.IsDeleted);

        // Mối quan hệ tùy chọn với Conversation
        builder.HasOne(v => v.Conversation)
               .WithMany()
               .HasForeignKey(v => v.ConversationID)
               .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ với User khởi tạo
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(v => v.InitiatorUserID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}



