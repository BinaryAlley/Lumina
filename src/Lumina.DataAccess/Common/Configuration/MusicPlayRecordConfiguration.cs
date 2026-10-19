#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="MusicPlayRecordEntity"/> entity.
/// </summary>
public class MusicPlayRecordConfiguration : IEntityTypeConfiguration<MusicPlayRecordEntity>
{
    /// <summary>
    /// Configures the <see cref="MusicPlayRecordEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<MusicPlayRecordEntity> builder)
    {
        builder.ToTable("MusicPlayRecords");
        builder.HasKey(playRecord => playRecord.Id);
        builder.Property(playRecord => playRecord.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(playRecord => playRecord.UserId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(playRecord => playRecord.TrackId)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(playRecord => playRecord.LibraryId)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(playRecord => playRecord.AlbumId)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(playRecord => playRecord.ArtistId)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(playRecord => playRecord.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(6);

        builder.Property(playRecord => playRecord.LastPositionMilliseconds)
            .IsRequired()
            .HasColumnOrder(7);

        builder.Property(playRecord => playRecord.ListenedDurationMilliseconds)
            .IsRequired()
            .HasColumnOrder(8);

        builder.Property(playRecord => playRecord.EndedOnUtc)
            .HasColumnOrder(9);

        // The version is the optimistic concurrency token: every update increments it, and an update whose loaded version no longer matches
        // the stored one affects no row and fails, instead of reverting a play that another report already completed or stopped.
        builder.Property(playRecord => playRecord.Version)
            .IsRequired()
            .IsConcurrencyToken()
            .HasColumnOrder(10);

        // Audit.
        builder.Property(playRecord => playRecord.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(11);

        builder.Property(playRecord => playRecord.CreatedBy)
            .IsRequired()
            .HasColumnOrder(12);

        builder.Property(playRecord => playRecord.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(13);

        builder.Property(playRecord => playRecord.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(14);

        // The referenced entities are kept by Id only, without foreign keys, so a play survives the deletion of the track it references and the
        // statistics of the listening history still count it.
        // Enables the chronological reads of the plays of a user, which the statistics are built from.
        builder.HasIndex(playRecord => new { playRecord.UserId, playRecord.CreatedOnUtc });

        // Enables the reads of the plays of a track, which the per track statistics are built from.
        builder.HasIndex(playRecord => playRecord.TrackId);
    }
}
