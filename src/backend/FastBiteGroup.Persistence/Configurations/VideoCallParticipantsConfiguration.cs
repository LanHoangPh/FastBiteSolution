namespace FastBiteGroup.Persistence.Configurations;

internal sealed class VideoCallParticipantsConfiguration : IEntityTypeConfiguration<VideoCallParticipants>
{
    public void Configure(EntityTypeBuilder<VideoCallParticipants> builder)
    {
        builder.ToTable(TableNames.VideoCallParticipants);
        builder.HasKey(x => x.VideoCallParticipantID);

        // Ràng buộc unique: Một user chỉ tham gia 1 session 1 lần
        builder.HasIndex(x => new { x.VideoCallSessionID, x.UserID }).IsUnique();
        builder.HasQueryFilter(x => x.VideoCallSession != null && !x.VideoCallSession.IsDeleted);

        // Mối quan hệ với VideoCallSession
        builder.HasOne(vp => vp.VideoCallSession)
               .WithMany(v => v.Participants)
               .HasForeignKey(vp => vp.VideoCallSessionID)
               .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ với User tham gia
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(vp => vp.UserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}


