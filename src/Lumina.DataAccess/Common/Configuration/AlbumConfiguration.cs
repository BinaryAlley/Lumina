#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="AlbumEntity"/> entity.
/// </summary>
public class AlbumConfiguration : IEntityTypeConfiguration<AlbumEntity>
{
    /// <summary>
    /// Configures the <see cref="AlbumEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<AlbumEntity> builder)
    {
        builder.ToTable("Albums");
        builder.HasKey(album => album.Id);
        builder.Property(album => album.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(album => album.ArtistId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(album => album.LibraryId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(album => album.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(3);
        builder.Property(album => album.OriginalTitle)
            .HasMaxLength(255)
            .HasColumnOrder(4);
        builder.Property(album => album.Description)
            .HasMaxLength(2000)
            .HasColumnOrder(5);
        builder.Property(album => album.OriginalReleaseDate)
            .HasColumnOrder(6);
        builder.Property(album => album.OriginalReleaseYear)
            .HasColumnOrder(7);
        builder.Property(album => album.ReReleaseDate)
            .HasColumnOrder(8);
        builder.Property(album => album.ReReleaseYear)
            .HasColumnOrder(9);
        builder.Property(album => album.ReleaseCountry)
            .HasConversion<string>()
            .HasMaxLength(2)
            .HasColumnOrder(10);
        builder.Property(album => album.ReleaseVersion)
            .HasColumnOrder(11);
        builder.Property(album => album.LanguageCode)
            .HasColumnOrder(12);
        builder.Property(album => album.LanguageName)
            .HasColumnOrder(13);
        builder.Property(album => album.LanguageNativeName)
            .HasColumnOrder(14);
        builder.Property(album => album.OriginalLanguageCode)
            .HasColumnOrder(15);
        builder.Property(album => album.OriginalLanguageName)
            .HasColumnOrder(16);
        builder.Property(album => album.OriginalLanguageNativeName)
            .HasColumnOrder(17);
        builder.Property(album => album.ReleaseType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(18);
        builder.Property(album => album.ReleaseStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(19);
        builder.Property(album => album.TotalDiscs)
            .HasColumnOrder(20);
        builder.Property(album => album.TotalTracks)
            .HasColumnOrder(21);
        builder.Property(album => album.MediaFormat)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(22);
        builder.Property(album => album.Barcode)
            .HasMaxLength(13)
            .HasColumnOrder(23);
        builder.Property(album => album.CatalogNumber)
            .HasMaxLength(50)
            .HasColumnOrder(24);
        builder.Property(album => album.MusicBrainzReleaseId)
            .HasColumnOrder(25);
        builder.Property(album => album.MusicBrainzReleaseGroupId)
            .HasColumnOrder(26);
        builder.Property(album => album.MusicBrainzReleaseArtistId)
            .HasColumnOrder(27);
        builder.Property(album => album.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(28);
        builder.Property(album => album.CreatedBy)
            .IsRequired()
            .HasColumnOrder(29);
        builder.Property(album => album.UpdatedOnUtc)
            .HasColumnOrder(30);
        builder.Property(album => album.UpdatedBy)
            .HasColumnOrder(31);

        builder.HasOne(album => album.Artist)
            .WithMany(artist => artist.Albums)
            .HasForeignKey(album => album.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // since Tag and Genre are Domain ValueObjects (no identity), but we also don't want to have duplicates,
        // we need to configure them as many-to-many relationships, where the tag/genre itself is the primary key
        builder.HasMany(album => album.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AlbumTags",
                j => j.HasOne<TagEntity>()
                    .WithMany()
                    .HasForeignKey("TagId"),
                j => j.HasOne<AlbumEntity>()
                    .WithMany()
                    .HasForeignKey("AlbumId"),
                j =>
                {
                    j.HasKey("AlbumId", "TagId");
                    j.ToTable("AlbumTags");
                });

        builder.HasMany(album => album.Genres)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AlbumGenres",
                j => j.HasOne<GenreEntity>()
                    .WithMany()
                    .HasForeignKey("GenreId"),
                j => j.HasOne<AlbumEntity>()
                    .WithMany()
                    .HasForeignKey("AlbumId"),
                j =>
                {
                    j.HasKey("AlbumId", "GenreId");
                    j.ToTable("AlbumGenres");
                });

        builder.OwnsMany(album => album.Ratings, ratingBuilder =>
        {
            ratingBuilder.ToTable("AlbumRatings");
            ratingBuilder.WithOwner()
                .HasForeignKey("AlbumId");
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

        builder.HasMany(album => album.Contributors)
            .WithOne()
            .HasForeignKey(contributor => contributor.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(album => new { album.ArtistId, album.Title });
        builder.HasIndex(album => new { album.LibraryId, album.Title });
    }
}
