using FastBiteGroup.Domain.Entities;
using FastBiteGroup.Persistence.Constants;
using FastBiteGroup.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastBiteGroup.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<AppRefreshToken>
{
    public void Configure(EntityTypeBuilder<AppRefreshToken> builder)
    {
        builder.ToTable(TableNames.RefreshTokens);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Jti)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.ExpiresAt).IsRequired();
        builder.Property(r => r.IsRevoked).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.RevokedAt).IsRequired(false);
        builder.Property(r => r.IsUsed).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.UsedAt).IsRequired(false);

        builder.Property(r => r.ReplacedByToken)
            .HasMaxLength(500);

        // Unique index on Token for fast lookup
        builder.HasIndex(r => r.Token).IsUnique();

        // Index on UserId for RevokeAllForUser queries
        builder.HasIndex(r => r.UserId);

        // FK to AppUser (in Identity tables)
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
