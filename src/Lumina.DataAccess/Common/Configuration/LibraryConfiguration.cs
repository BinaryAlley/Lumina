#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="LibraryEntity"/> entity.
/// </summary>
public class LibraryConfiguration : IEntityTypeConfiguration<LibraryEntity>
{
    /// <summary>
    /// Configures the <see cref="LibraryEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<LibraryEntity> builder)
    {
        builder.ToTable("Libraries");
        builder.HasKey(library => library.Id);
        builder.Property(library => library.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(library => library.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(1);
        
        builder.Property(library => library.LibraryType)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(2);

        builder.Property(library => library.CoverImage)
            .HasMaxLength(255)
            .HasDefaultValue(null)
            .HasColumnOrder(3);

        builder.Property(library => library.IsEnabled)
            .HasDefaultValue(true)
            .HasColumnOrder(4);

        builder.Property(library => library.IsLocked)
            .HasColumnOrder(5);

        builder.Property(library => library.DownloadMedatadaFromWeb)
            .HasDefaultValue(true)
            .HasColumnOrder(6);

        builder.Property(library => library.SaveMetadataInMediaDirectories)
            .HasColumnOrder(7);

        // one user with many libraries
        builder.HasOne(library => library.User)
            .WithMany(user => user.Libraries)
            .HasForeignKey(library => library.UserId)
            .IsRequired();

        // one library with many scans
        builder.HasMany(library => library.LibraryScans)
              .WithOne(libraryScan => libraryScan.Library)
              .HasForeignKey(libraryScan => libraryScan.LibraryId)
              .OnDelete(DeleteBehavior.Cascade); // deleting a library deletes library scans

        // using OwnsMany because paths are domain Value Objects, thus they have no independent identity, and their lifecycle is bound to the Library
        builder.OwnsMany(library => library.ContentLocations, contentLocationBuilder =>
        {
            contentLocationBuilder.ToTable("LibraryContentLocations");
            contentLocationBuilder.WithOwner()
                .HasForeignKey("LibraryId");

            contentLocationBuilder.Property<Guid>("Id")
                .ValueGeneratedOnAdd();
            contentLocationBuilder.HasKey("Id");

            contentLocationBuilder.Property(location => location.Path)
                 .HasColumnName("Path")
                 .HasMaxLength(260)
                 .IsRequired();
        });

        // audit
        builder.Property(library => library.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(8);

        builder.Property(library => library.CreatedBy)
            .IsRequired()
            .HasColumnOrder(9);

        builder.Property(library => library.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(10);

        builder.Property(library => library.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(11);
    }
}
