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
        builder.Property(artist => artist.SortName)
            .HasMaxLength(255)
            .HasColumnOrder(3);
        builder.Property(artist => artist.Disambiguation)
            .HasMaxLength(255)
            .HasColumnOrder(4);
        builder.Property(artist => artist.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(5);
        builder.Property(artist => artist.Gender)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(6);
        builder.Property(artist => artist.Country)
            .HasMaxLength(2)
            .HasColumnOrder(7);
        builder.Property(artist => artist.LifeSpanBegin)
            .HasColumnOrder(8);
        builder.Property(artist => artist.LifeSpanEnd)
            .HasColumnOrder(9);
        builder.Property(artist => artist.IsEnded)
            .HasColumnOrder(10);
        builder.Property(artist => artist.Website)
            .HasMaxLength(2048)
            .HasColumnOrder(11);
        builder.Property(artist => artist.MusicBrainzArtistId)
            .HasColumnOrder(12);
        builder.Property(artist => artist.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(13);
        builder.Property(artist => artist.CreatedBy)
            .IsRequired()
            .HasColumnOrder(14);
        builder.Property(artist => artist.UpdatedOnUtc)
            .HasColumnOrder(15);
        builder.Property(artist => artist.UpdatedBy)
            .HasColumnOrder(16);
        builder.Property(artist => artist.MetadataStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(MetadataStatus.Pending)
            .HasColumnOrder(17);
        builder.Property(artist => artist.LastMetadataUpdateUtc)
            .HasColumnOrder(18);
        builder.Property(artist => artist.MetadataProvider)
            .HasMaxLength(100)
            .HasColumnOrder(19);

        // The areas are owned value objects that are never referenced on their own, so they are stored as JSON documents next to the artist.
        builder.OwnsOne(artist => artist.Area, areaBuilder => areaBuilder.ToJson());
        builder.OwnsOne(artist => artist.BeginArea, beginAreaBuilder => beginAreaBuilder.ToJson());
        builder.OwnsOne(artist => artist.EndArea, endAreaBuilder => endAreaBuilder.ToJson());

        builder.OwnsMany(artist => artist.Aliases, aliasBuilder =>
        {
            aliasBuilder.ToTable("ArtistAliases");
            aliasBuilder.WithOwner()
                .HasForeignKey("ArtistId");
            aliasBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            aliasBuilder.HasKey("Id");

            aliasBuilder.Property(alias => alias.Name)
                .IsRequired()
                .HasMaxLength(255);
            aliasBuilder.Property(alias => alias.SortName)
                .HasMaxLength(255);
            aliasBuilder.Property(alias => alias.Type)
                .HasMaxLength(50);
            aliasBuilder.Property(alias => alias.Locale)
                .HasMaxLength(20);
        });

        builder.OwnsMany(artist => artist.Ipis, ipiBuilder =>
        {
            ipiBuilder.ToTable("ArtistIpis");
            ipiBuilder.WithOwner()
                .HasForeignKey("ArtistId");
            ipiBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            ipiBuilder.HasKey("Id");

            ipiBuilder.Property(ipi => ipi.Value)
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.OwnsMany(artist => artist.Isnis, isniBuilder =>
        {
            isniBuilder.ToTable("ArtistIsnis");
            isniBuilder.WithOwner()
                .HasForeignKey("ArtistId");
            isniBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            isniBuilder.HasKey("Id");

            isniBuilder.Property(isni => isni.Value)
                .IsRequired()
                .HasMaxLength(20);
        });

        // since Tag and Genre are Domain ValueObjects (no identity), but we also don't want to have duplicates,
        // we need to configure them as many-to-many relationships, where the tag/genre itself is the primary key
        builder.HasMany(artist => artist.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ArtistTags",
                j => j.HasOne<TagEntity>()
                    .WithMany()
                    .HasForeignKey("TagId"),
                j => j.HasOne<ArtistEntity>()
                    .WithMany()
                    .HasForeignKey("ArtistId"),
                j =>
                {
                    j.HasKey("ArtistId", "TagId");
                    j.ToTable("ArtistTags");
                });

        builder.HasMany(artist => artist.Genres)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ArtistGenres",
                j => j.HasOne<GenreEntity>()
                    .WithMany()
                    .HasForeignKey("GenreId"),
                j => j.HasOne<ArtistEntity>()
                    .WithMany()
                    .HasForeignKey("ArtistId"),
                j =>
                {
                    j.HasKey("ArtistId", "GenreId");
                    j.ToTable("ArtistGenres");
                });

        builder.OwnsMany(artist => artist.Ratings, ratingBuilder =>
        {
            ratingBuilder.ToTable("ArtistRatings");
            ratingBuilder.WithOwner()
                .HasForeignKey("ArtistId");
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

        builder.HasMany(artist => artist.Contributors)
            .WithOne()
            .HasForeignKey(contributor => contributor.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(artist => new { artist.LibraryId, artist.Name })
            .IsUnique(); // the same artist cannot appear twice in the same library
    }
}
