#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="FavoriteEntity"/> entity.
/// </summary>
public class FavoriteConfiguration : IEntityTypeConfiguration<FavoriteEntity>
{
    /// <summary>
    /// Configures the <see cref="FavoriteEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<FavoriteEntity> builder)
    {
        builder.ToTable("Favorites");
        builder.HasKey(favorite => favorite.Id);
        builder.Property(favorite => favorite.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        // A favorite belongs to the user that created it, and it is removed with that user.
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(favorite => favorite.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(favorite => favorite.UserId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(favorite => favorite.MediaKind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(2);

        builder.Property(favorite => favorite.MediaItemId)
            .IsRequired()
            .HasColumnOrder(3);

        // Audit.
        builder.Property(favorite => favorite.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(favorite => favorite.CreatedBy)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(favorite => favorite.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(6);

        builder.Property(favorite => favorite.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(7);

        // The referenced media item is kept by kind and Id only, without a foreign key: one table serves every media kind, so no single table
        // can be referenced, and a favorite survives the deletion of the item it references, so its owner can still see and remove it.
        // The unique index makes the storage medium the owner of the "one favorite per user and item" invariant: a duplicate is rejected even
        // when two concurrent adds both pass the existence check of the handler.
        builder.HasIndex(favorite => new { favorite.UserId, favorite.MediaKind, favorite.MediaItemId })
            .IsUnique();

        // Enable listing the favorites of a user from the most recently favorited: the first index serves the listing of every kind, and the second one serves the listing restricted to one media kind.
        builder.HasIndex(favorite => new { favorite.UserId, favorite.CreatedOnUtc });

        builder.HasIndex(favorite => new { favorite.UserId, favorite.MediaKind, favorite.CreatedOnUtc });
    }
}
