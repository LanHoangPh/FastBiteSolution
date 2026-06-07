using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastBiteGroup.Persistentce.Configurations;

internal sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable(TableNames.WorkspaceMembers);
        builder.HasKey(x => x.WorkspaceMemberID);

        // Ràng buộc unique: Một user chỉ tham gia 1 Workspace 1 lần
        builder.HasIndex(x => new { x.WorkspaceID, x.UserID }).IsUnique();

        // Cấu hình Enum
        builder.Property(x => x.Role)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(x => x.JoinedAt).IsRequired();
        builder.Property(x => x.LeftAt).IsRequired(false);

        // Mối quan hệ với Workspace
        builder.HasOne(gm => gm.Workspace)
               .WithMany(g => g.Members)
               .HasForeignKey(gm => gm.WorkspaceID)
               .OnDelete(DeleteBehavior.Cascade);

        // Mối quan hệ với User
        builder.HasOne<AppUser>()
               .WithMany()
               .HasForeignKey(gm => gm.UserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
