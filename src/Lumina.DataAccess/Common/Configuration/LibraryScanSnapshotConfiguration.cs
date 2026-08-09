#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="LibraryScanSnapshotEntity"/> entity.
/// </summary>
public class LibraryScanSnapshotConfiguration : IEntityTypeConfiguration<LibraryScanSnapshotEntity>
{
    /// <summary>
    /// Configures the <see cref="LibraryScanSnapshotEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<LibraryScanSnapshotEntity> builder)
    {
        builder.ToTable("LibraryScanSnapshots");
        // composite key, so that each media library scan snapshot item is uniquely identified within its library by its path
        builder.HasKey(libraryScanSnapshot => new { libraryScanSnapshot.LibraryId, libraryScanSnapshot.Path });

        // one library with many media library scan snapshot items
        builder.HasOne(libraryScanSnapshot => libraryScanSnapshot.Library)
            .WithMany()
            .HasForeignKey(libraryScanSnapshot => libraryScanSnapshot.LibraryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.Path)
            .HasMaxLength(1024)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.ContentHash)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.FileSize)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.Ticks)
            .IsRequired()
            .HasColumnOrder(4);

        // audit
        builder.Property(libraryScanSnapshot => libraryScanSnapshot.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.CreatedBy)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(7);

        builder.Property(libraryScanSnapshot => libraryScanSnapshot.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        // enables fast lookups of the media library scan snapshot items of a library
        builder.HasIndex(libraryScanSnapshot => libraryScanSnapshot.LibraryId);
    }
}
