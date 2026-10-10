#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="MusicArtworkEntity"/> entity.
/// </summary>
public class MusicArtworkConfiguration : IEntityTypeConfiguration<MusicArtworkEntity>
{
    /// <summary>
    /// Configures the <see cref="MusicArtworkEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<MusicArtworkEntity> builder)
    {
        builder.ToTable("MusicArtwork");
        builder.HasKey(musicArtwork => musicArtwork.Id);
        builder.Property(musicArtwork => musicArtwork.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);
        builder.Property(musicArtwork => musicArtwork.OwnerType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(1);
        builder.Property(musicArtwork => musicArtwork.OwnerId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(musicArtwork => musicArtwork.ArtworkType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(3);
        builder.Property(musicArtwork => musicArtwork.Ordinal)
            .IsRequired()
            .HasColumnOrder(4);
        builder.Property(musicArtwork => musicArtwork.FileName)
            .HasMaxLength(2048)
            .HasColumnOrder(5);
        builder.Property(musicArtwork => musicArtwork.ContentHash)
            .HasColumnOrder(6);
        builder.Property(musicArtwork => musicArtwork.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(7);
        builder.Property(musicArtwork => musicArtwork.Provider)
            .HasMaxLength(100)
            .HasColumnOrder(8);
        builder.Property(musicArtwork => musicArtwork.LastUpdateUtc)
            .HasColumnOrder(9);

        // audit
        builder.Property(musicArtwork => musicArtwork.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(10);

        builder.Property(musicArtwork => musicArtwork.CreatedBy)
            .IsRequired()
            .HasColumnOrder(11);

        builder.Property(musicArtwork => musicArtwork.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(12);

        builder.Property(musicArtwork => musicArtwork.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(13);

        builder.HasIndex(musicArtwork => new { musicArtwork.OwnerType, musicArtwork.OwnerId, musicArtwork.ArtworkType, musicArtwork.Ordinal })
            .IsUnique();
        builder.HasIndex(musicArtwork => musicArtwork.Status);
    }
}
