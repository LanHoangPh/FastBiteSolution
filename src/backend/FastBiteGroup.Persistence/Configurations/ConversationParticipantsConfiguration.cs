namespace FastBiteGroup.Persistence.Configurations;

internal sealed class ConversationParticipantsConfiguration : IEntityTypeConfiguration<ConversationParticipants>
{
    public void Configure(EntityTypeBuilder<ConversationParticipants> builder)
    {
        builder.ToTable(TableNames.ConversationParticipants);
        builder.HasKey(x => x.ConversationParticipantID);

        // Ràng buộc unique
        builder.HasIndex(x => new { x.ConversationID, x.UserID }).IsUnique();
        builder.HasQueryFilter(x => x.Conversation != null && !x.Conversation.IsDeleted);

        // Mối quan hệ với Conversation
        builder.HasOne(cp => cp.Conversation)
               .WithMany(c => c.Participants)
               .HasForeignKey(cp => cp.ConversationID)
               .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ với User
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(cp => cp.UserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}


