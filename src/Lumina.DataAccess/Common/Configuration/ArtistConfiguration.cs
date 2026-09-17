#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="ArtistEntity"/> entity.
/// </summary>
public class ArtistConfiguration : IEntityTypeConfiguration<ArtistEntity>
{
    /// <summary>
    /// Configures the <see cref="ArtistEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<ArtistEntity> builder)
    {
        builder.ToTable("Artists");
        builder.HasKey(artist => artist.Id);
        builder.Property(artist => artist.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(artist => artist.LibraryId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(artist => artist.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(2);
        builder.Property(artist => artist.Website)
            .HasMaxLength(2048)
            .HasColumnOrder(3);
        builder.Property(artist => artist.MusicBrainzArtistId)
            .HasColumnOrder(4);
        builder.Property(artist => artist.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(5);
        builder.Property(artist => artist.CreatedBy)
            .IsRequired()
            .HasColumnOrder(6);
        builder.Property(artist => artist.UpdatedOnUtc)
            .HasColumnOrder(7);
        builder.Property(artist => artist.UpdatedBy)
            .HasColumnOrder(8);

        builder.HasMany(artist => artist.Contributors)
            .WithOne()
            .HasForeignKey(contributor => contributor.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(artist => new { artist.LibraryId, artist.Name })
            .IsUnique(); // the same artist cannot appear twice in the same library
    }
}
