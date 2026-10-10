#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="MusicLibraryScanItemMetadataEntity"/> entity.
/// </summary>
public class MusicLibraryScanItemMetadataConfiguration : IEntityTypeConfiguration<MusicLibraryScanItemMetadataEntity>
{
    /// <summary>
    /// Configures the <see cref="MusicLibraryScanItemMetadataEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<MusicLibraryScanItemMetadataEntity> builder)
    {
        builder.ToTable("MusicLibraryScanItemMetadata");
        builder.HasKey(musicMetadata => musicMetadata.Id);
        builder.Property(musicMetadata => musicMetadata.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);
        builder.Property(musicMetadata => musicMetadata.LibraryScanId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(musicMetadata => musicMetadata.LibraryId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(musicMetadata => musicMetadata.Path)
            .IsRequired()
            .HasMaxLength(2048)
            .HasColumnOrder(3);
        builder.Property(musicMetadata => musicMetadata.ArtistName)
            .HasMaxLength(255)
            .HasColumnOrder(4);
        builder.Property(musicMetadata => musicMetadata.ReleaseType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(5);
        builder.Property(musicMetadata => musicMetadata.ReleaseYear)
            .HasColumnOrder(6);
        builder.Property(musicMetadata => musicMetadata.ReleaseName)
            .HasMaxLength(255)
            .HasColumnOrder(7);
        builder.Property(musicMetadata => musicMetadata.TrackTitle)
            .HasMaxLength(255)
            .HasColumnOrder(8);
        builder.Property(musicMetadata => musicMetadata.TrackNumber)
            .HasColumnOrder(9);
        builder.Property(musicMetadata => musicMetadata.DiscNumber)
            .HasColumnOrder(10);
        builder.Property(musicMetadata => musicMetadata.DurationInSeconds)
            .HasColumnOrder(11);
        builder.Property(musicMetadata => musicMetadata.SampleRate)
            .HasColumnOrder(12);
        builder.Property(musicMetadata => musicMetadata.Channels)
            .HasColumnOrder(13);
        builder.Property(musicMetadata => musicMetadata.BitDepth)
            .HasColumnOrder(14);
        builder.Property(musicMetadata => musicMetadata.AudioCodec)
            .HasMaxLength(50)
            .HasColumnOrder(15);
        builder.Property(musicMetadata => musicMetadata.Bitrate)
            .HasColumnOrder(16);
        builder.Property(musicMetadata => musicMetadata.MusicBrainzArtistId)
            .HasColumnOrder(17);
        builder.Property(musicMetadata => musicMetadata.MusicBrainzReleaseArtistId)
            .HasColumnOrder(18);
        builder.Property(musicMetadata => musicMetadata.MusicBrainzReleaseGroupId)
            .HasColumnOrder(19);
        builder.Property(musicMetadata => musicMetadata.MusicBrainzReleaseId)
            .HasColumnOrder(20);
        builder.Property(musicMetadata => musicMetadata.MusicBrainzRecordingId)
            .HasColumnOrder(21);
        builder.Property(musicMetadata => musicMetadata.MusicBrainzTrackId)
            .HasColumnOrder(22);
        builder.Property(musicMetadata => musicMetadata.MusicBrainzWorkId)
            .HasColumnOrder(23);
        builder.Property(musicMetadata => musicMetadata.WorkTitle)
            .HasMaxLength(255)
            .HasColumnOrder(24);
        builder.Property(musicMetadata => musicMetadata.AcoustId)
            .HasMaxLength(64)
            .HasColumnOrder(25);
        builder.Property(musicMetadata => musicMetadata.ReplayGainTrackGain)
            .HasColumnType("decimal(8,2)")
            .HasColumnOrder(26);
        builder.Property(musicMetadata => musicMetadata.ReplayGainTrackPeak)
            .HasColumnType("decimal(8,6)")
            .HasColumnOrder(27);
        builder.Property(musicMetadata => musicMetadata.ReplayGainAlbumGain)
            .HasColumnType("decimal(8,2)")
            .HasColumnOrder(28);
        builder.Property(musicMetadata => musicMetadata.ReplayGainAlbumPeak)
            .HasColumnType("decimal(8,6)")
            .HasColumnOrder(29);
        builder.PrimitiveCollection(musicMetadata => musicMetadata.Moods)
            .HasColumnOrder(30);

        builder.HasIndex(musicMetadata => new { musicMetadata.LibraryScanId, musicMetadata.Path })
            .IsUnique(); // the metadata of the same physical file cannot be staged twice during the same scan
    }
}
