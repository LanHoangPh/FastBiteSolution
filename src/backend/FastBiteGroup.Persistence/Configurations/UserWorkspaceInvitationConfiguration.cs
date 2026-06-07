using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastBiteGroup.Persistentce.Configurations;

internal class UserWorkspaceInvitationConfiguration : IEntityTypeConfiguration<UserWorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<UserWorkspaceInvitation> builder)
    {
        builder.ToTable(TableNames.UserWorkspaceInvitations);
        builder.HasKey(x => x.InvitationID);

        // Ràng buộc unique: Một user chỉ có thể nhận một lời mời từ một nhóm cụ thể
        builder.HasIndex(x => new { x.InvitedUserID, x.WorkspaceID })
               .IsUnique()
               .HasDatabaseName("IX_Unique_User_Workspace_Invitation");
        // Cấu hình các thuộc tính
        builder.Property(x => x.InvitationID)
               .ValueGeneratedOnAdd();
        builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>(); // Chuyển đổi Enum sang string trong CSDL
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.RespondedAt).IsRequired(false);
        // Cấu hình mối quan hệ với User và Workspace
        builder.HasOne(invitation => invitation.Workspace)
               .WithMany(workspace => workspace.DirectUserInvitations)
               .HasForeignKey(invitation => invitation.WorkspaceID)
               .OnDelete(DeleteBehavior.Cascade); // Nếu xóa Workspace, các lời mời liên quan cũng sẽ bị xóa

        // 2. Mối quan hệ với AppUser với vai trò "Người được mời"
        builder.HasOne<AppUser>()
               .WithMany() // Trỏ đến collection các lời mời user "nhận được"
               .HasForeignKey(invitation => invitation.InvitedUserID)
               .OnDelete(DeleteBehavior.Restrict); // Ngăn việc xóa một user nếu họ có lời mời đang chờ

        // 3. Mối quan hệ với AppUser với vai trò "Người mời"
        builder.HasOne<AppUser>()
               .WithMany() // Trỏ đến collection các lời mời user "đã gửi"
               .HasForeignKey(invitation => invitation.InvitedByUserID)
               .OnDelete(DeleteBehavior.Restrict); // Ngăn việc xóa một user nếu họ đã gửi lời mời

    }
}
