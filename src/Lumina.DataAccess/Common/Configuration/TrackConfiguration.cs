#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="TrackEntity"/> entity.
/// </summary>
public class TrackConfiguration : IEntityTypeConfiguration<TrackEntity>
{
    /// <summary>
    /// Configures the <see cref="TrackEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TrackEntity> builder)
    {
        builder.ToTable("Tracks");
        builder.HasKey(track => track.Id);
        builder.Property(track => track.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(track => track.AlbumId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(track => track.LibraryId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(track => track.Path)
            .IsRequired()
            .HasMaxLength(2048)
            .HasColumnOrder(3);
        builder.Property(track => track.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(4);
        builder.Property(track => track.OriginalTitle)
            .HasMaxLength(255)
            .HasColumnOrder(5);
        builder.Property(track => track.Description)
            .HasMaxLength(2000)
            .HasColumnOrder(6);
        builder.Property(track => track.Disambiguation)
            .HasMaxLength(255)
            .HasColumnOrder(7);
        builder.Property(track => track.OriginalReleaseDate)
            .HasColumnOrder(8);
        builder.Property(track => track.OriginalReleaseYear)
            .HasColumnOrder(9);
        builder.Property(track => track.ReReleaseDate)
            .HasColumnOrder(10);
        builder.Property(track => track.ReReleaseYear)
            .HasColumnOrder(11);
        builder.Property(track => track.ReleaseCountry)
            .HasConversion<string>()
            .HasMaxLength(2)
            .HasColumnOrder(12);
        builder.Property(track => track.ReleaseVersion)
            .HasColumnOrder(13);
        builder.Property(track => track.LanguageCode)
            .HasColumnOrder(14);
        builder.Property(track => track.LanguageName)
            .HasColumnOrder(15);
        builder.Property(track => track.LanguageNativeName)
            .HasColumnOrder(16);
        builder.Property(track => track.OriginalLanguageCode)
            .HasColumnOrder(17);
        builder.Property(track => track.OriginalLanguageName)
            .HasColumnOrder(18);
        builder.Property(track => track.OriginalLanguageNativeName)
            .HasColumnOrder(19);
        builder.Property(track => track.DurationInSeconds)
            .HasColumnOrder(20);
        builder.Property(track => track.SampleRate)
            .HasColumnOrder(21);
        builder.Property(track => track.Channels)
            .HasColumnOrder(22);
        builder.Property(track => track.BitDepth)
            .HasColumnOrder(23);
        builder.Property(track => track.AudioCodec)
            .HasMaxLength(50)
            .HasColumnOrder(24);
        builder.Property(track => track.Bitrate)
            .HasColumnOrder(25);
        builder.Property(track => track.TrackNumber)
            .HasColumnOrder(26);
        builder.Property(track => track.DiscNumber)
            .HasColumnOrder(27);
        builder.Property(track => track.Script)
            .HasMaxLength(50)
            .HasColumnOrder(28);
        builder.Property(track => track.Key)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(29);
        builder.Property(track => track.Bpm)
            .HasColumnOrder(30);
        builder.Property(track => track.IsVideo)
            .HasColumnOrder(31);
        builder.Property(track => track.WorkTitle)
            .HasMaxLength(255)
            .HasColumnOrder(32);
        builder.Property(track => track.WorkType)
            .HasMaxLength(50)
            .HasColumnOrder(33);
        builder.Property(track => track.MusicBrainzRecordingId)
            .HasColumnOrder(34);
        builder.Property(track => track.MusicBrainzTrackId)
            .HasColumnOrder(35);
        builder.Property(track => track.MusicBrainzWorkId)
            .HasColumnOrder(36);
        builder.Property(track => track.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(37);
        builder.Property(track => track.CreatedBy)
            .IsRequired()
            .HasColumnOrder(38);
        builder.Property(track => track.UpdatedOnUtc)
            .HasColumnOrder(39);
        builder.Property(track => track.UpdatedBy)
            .HasColumnOrder(40);
        builder.Property(track => track.MetadataStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(MetadataStatus.Pending)
            .HasColumnOrder(41);
        builder.Property(track => track.LastMetadataUpdateUtc)
            .HasColumnOrder(42);
        builder.Property(track => track.MetadataProvider)
            .HasMaxLength(100)
            .HasColumnOrder(43);
        builder.Property(track => track.AcoustId)
            .HasMaxLength(64)
            .HasColumnOrder(44);
        builder.Property(track => track.ReplayGainTrackGain)
            .HasColumnType("decimal(8,2)")
            .HasColumnOrder(45);
        builder.Property(track => track.ReplayGainTrackPeak)
            .HasColumnType("decimal(8,6)")
            .HasColumnOrder(46);
        builder.Property(track => track.ReplayGainAlbumGain)
            .HasColumnType("decimal(8,2)")
            .HasColumnOrder(47);
        builder.Property(track => track.ReplayGainAlbumPeak)
            .HasColumnType("decimal(8,6)")
            .HasColumnOrder(48);

        builder.HasOne(track => track.Album)
            .WithMany(album => album.Tracks)
            .HasForeignKey(track => track.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        // since Tag and Genre are Domain ValueObjects (no identity), but we also don't want to have duplicates,
        // we need to configure them as many-to-many relationships, where the tag/genre itself is the primary key
        builder.HasMany(track => track.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TrackTags",
                j => j.HasOne<TagEntity>()
                    .WithMany()
                    .HasForeignKey("TagId"),
                j => j.HasOne<TrackEntity>()
                    .WithMany()
                    .HasForeignKey("TrackId"),
                j =>
                {
                    j.HasKey("TrackId", "TagId");
                    j.ToTable("TrackTags");
                });

        builder.HasMany(track => track.Genres)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TrackGenres",
                j => j.HasOne<GenreEntity>()
                    .WithMany()
                    .HasForeignKey("GenreId"),
                j => j.HasOne<TrackEntity>()
                    .WithMany()
                    .HasForeignKey("TrackId"),
                j =>
                {
                    j.HasKey("TrackId", "GenreId");
                    j.ToTable("TrackGenres");
                });

        builder.OwnsMany(track => track.Moods, moodBuilder =>
        {
            moodBuilder.ToTable("TrackMoods");
            moodBuilder.WithOwner()
                .HasForeignKey("TrackId");
            moodBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            moodBuilder.HasKey("Id");

            moodBuilder.Property(mood => mood.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.OwnsMany(track => track.Isrcs, isrcBuilder =>
        {
            isrcBuilder.ToTable("TrackIsrcs");
            isrcBuilder.WithOwner()
                .HasForeignKey("TrackId");
            isrcBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            isrcBuilder.HasKey("Id");

            isrcBuilder.Property(isrc => isrc.Value)
                .HasColumnName("ISRC")
                .HasMaxLength(12)
                .IsRequired();
        });

        builder.OwnsMany(track => track.WorkLanguages, workLanguageBuilder =>
        {
            workLanguageBuilder.ToTable("TrackWorkLanguages");
            workLanguageBuilder.WithOwner()
                .HasForeignKey("TrackId");
            workLanguageBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            workLanguageBuilder.HasKey("Id");

            workLanguageBuilder.Property(workLanguage => workLanguage.LanguageCode)
                .IsRequired()
                .HasMaxLength(5);
            workLanguageBuilder.Property(workLanguage => workLanguage.LanguageName)
                .IsRequired()
                .HasMaxLength(100);
            workLanguageBuilder.Property(workLanguage => workLanguage.NativeName)
                .HasMaxLength(100);
        });

        builder.OwnsMany(track => track.WorkIswcs, workIswcBuilder =>
        {
            workIswcBuilder.ToTable("TrackWorkIswcs");
            workIswcBuilder.WithOwner()
                .HasForeignKey("TrackId");
            workIswcBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            workIswcBuilder.HasKey("Id");

            workIswcBuilder.Property(workIswc => workIswc.Value)
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.HasMany(track => track.Contributors)
            .WithOne()
            .HasForeignKey(contributor => contributor.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(track => track.Ratings, ratingBuilder =>
        {
            ratingBuilder.ToTable("TrackRatings");
            ratingBuilder.WithOwner()
                .HasForeignKey("TrackId");
            ratingBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            ratingBuilder.HasKey("Id");

            ratingBuilder.Property(rating => rating.Value)
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            ratingBuilder.Property(rating => rating.MaxValue)
                .HasColumnType("decimal(3,2)")
                .IsRequired();

            ratingBuilder.Property(rating => rating.VoteCount)
                .IsRequired(false);

            ratingBuilder.Property(rating => rating.Source)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired(false);
        });

        builder.HasIndex(track => new { track.AlbumId, track.TrackNumber });
        builder.HasIndex(track => new { track.LibraryId, track.Path })
            .IsUnique(); // the same physical track file cannot appear twice in the same library
    }
}
