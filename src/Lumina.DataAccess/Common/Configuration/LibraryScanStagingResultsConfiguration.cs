#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="LibraryScanStagingResultsEntity"/> entity.
/// </summary>
public class LibraryScanStagingResultsConfiguration : IEntityTypeConfiguration<LibraryScanStagingResultsEntity>
{
    /// <summary>
    /// Configures the <see cref="LibraryScanStagingResultsEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<LibraryScanStagingResultsEntity> builder)
    {
        builder.ToTable("LibraryScanStagingResults");
        // composite key, so that each media library scan staging result is uniquely identified within its scan by its path
        builder.HasKey(libraryScanStagingResult => new { libraryScanStagingResult.LibraryScanId, libraryScanStagingResult.Path });

        // one media library scan with many media library scan staging results
        builder.HasOne<LibraryScanEntity>()
            .WithMany()
            .HasForeignKey(libraryScanStagingResult => libraryScanStagingResult.LibraryScanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.Path)
            .HasMaxLength(1024)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.Size)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.Ticks)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.ContentHash)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.PreviousContentHash)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.NeedsRehash)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(libraryScanStagingResult => libraryScanStagingResult.IsNew)
            .IsRequired()
            .HasColumnOrder(7);

        // enables fast lookups of the staging results of a scan that need their content hashed
        builder.HasIndex(libraryScanStagingResult => new { libraryScanStagingResult.LibraryScanId, libraryScanStagingResult.NeedsRehash });
    }
}
