namespace FastBiteGroup.Persistentce.Configurations;

internal sealed class ConversationsConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable(TableNames.Conversations);
        builder.HasKey(x => x.ConversationID);

        builder.Property(x => x.Title).HasMaxLength(255);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);

        builder.Property(x => x.WorkspaceID).IsRequired(false);
        builder.HasIndex(x => x.WorkspaceID)
               .IsUnique()
               .HasFilter("\"WorkspaceID\" IS NOT NULL");

        builder.Property(x => x.ConversationType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.HasOne(c => c.Workspace)
               .WithOne(g => g.Conversation)
               .HasForeignKey<Conversation>(c => c.WorkspaceID)
               .IsRequired(false);

        // Cấu hình query filter cho Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
