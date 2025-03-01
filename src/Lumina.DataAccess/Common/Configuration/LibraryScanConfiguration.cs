#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="LibraryScanEntity"/> entity.
/// </summary>
public class LibraryScanConfiguration : IEntityTypeConfiguration<LibraryScanEntity>
{
    /// <summary>
    /// Configures the <see cref="LibraryScanEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<LibraryScanEntity> builder)
    {
        builder.ToTable("LibraryScans");
        builder.HasKey(library => library.Id);
        builder.Property(library => library.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        // one user with many library scans
        builder.HasOne(libraryScan => libraryScan.User)
            .WithMany(user => user.LibraryScans)
            .HasForeignKey(libraryScan => libraryScan.UserId)
            .IsRequired();

        // one library with many library scans
        builder.HasOne(libraryScan => libraryScan.Library)
            .WithMany(library => library.LibraryScans)
            .HasForeignKey(libraryScan => libraryScan.LibraryId)
            .IsRequired();

        builder.Property(libraryScan => libraryScan.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(1);

        // audit
        builder.Property(libraryScan => libraryScan.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(libraryScan => libraryScan.CreatedBy)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(libraryScan => libraryScan.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(4);

        builder.Property(libraryScan => libraryScan.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(5);
    }
}
