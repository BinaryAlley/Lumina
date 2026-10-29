#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="PlaylistEntryEntity"/> entity.
/// </summary>
public class PlaylistEntryConfiguration : IEntityTypeConfiguration<PlaylistEntryEntity>
{
    /// <summary>
    /// Configures the <see cref="PlaylistEntryEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<PlaylistEntryEntity> builder)
    {
        builder.ToTable("PlaylistEntries");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(entry => entry.PlaylistId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(entry => entry.MediaItemId)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(entry => entry.MediaItemType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(3);

        builder.Property(entry => entry.Position)
            .IsRequired()
            .HasColumnOrder(4);

        // Audit.
        builder.Property(entry => entry.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(entry => entry.CreatedBy)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(entry => entry.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(7);

        builder.Property(entry => entry.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        // The referenced media item is kept by Id only, without a foreign key, so a playlist entry survives the deletion of the item it
        // references and its owner can still see and remove the dangling entry.
        // The unique index makes the storage medium the owner of the ordering invariant: a duplicate position is rejected even if an operation bypasses the repository.
        builder.HasIndex(entry => new { entry.PlaylistId, entry.Position })
            .IsUnique();

        // Enables finding the playlists that reference a media item, for example when a track is deleted.
        builder.HasIndex(entry => entry.MediaItemId);
    }
}
