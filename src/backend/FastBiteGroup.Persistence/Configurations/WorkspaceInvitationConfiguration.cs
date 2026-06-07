using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastBiteGroup.Persistentce.Configurations;

internal sealed class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
    {
        builder.ToTable(TableNames.WorkspaceInvitations);
        builder.HasKey(x => x.InvitationID);

        builder.Property(x => x.InvitationCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        builder.HasIndex(x => x.InvitationCode).IsUnique();

        // Mối quan hệ với Workspace
        builder.HasOne(gi => gi.Workspace)
               .WithMany(g => g.WorkspaceInvitations)
               .HasForeignKey(gi => gi.WorkspaceID)
               .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ với User tạo link
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(gi => gi.CreatedByUserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
