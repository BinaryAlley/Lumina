#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="LibraryScanResultEntity"/> entity.
/// </summary>
public class LibraryScanResultConfiguration : IEntityTypeConfiguration<LibraryScanResultEntity>
{
    /// <summary>
    /// Configures the <see cref="LibraryScanResultEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<LibraryScanResultEntity> builder)
    {
        builder.ToTable("LibraryScanResults");
        // composite key for efficient scan-specific lookups
        builder.HasKey(libraryScanResult => new { libraryScanResult.LibraryScanId, libraryScanResult.Path });

        // one library scan with many library scan results
        builder.HasOne(libraryScanResult => libraryScanResult.LibraryScan)
            .WithMany()
            .HasForeignKey(libraryScanResult => libraryScanResult.LibraryScanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(libraryScanResult => libraryScanResult.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(libraryScanResult => libraryScanResult.Path)
            .HasMaxLength(1024)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(libraryScanResult => libraryScanResult.ContentHash)
            .HasMaxLength(24)  // xxHash128 as Base64
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(libraryScanResult => libraryScanResult.FileSize)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(libraryScanResult => libraryScanResult.LastModified)
            .IsRequired()
            .HasColumnOrder(4);

        // Indexes
        builder.HasIndex(libraryScanResult => new { libraryScanResult.ContentHash, libraryScanResult.FileSize, libraryScanResult.Path })
            .IsUnique();

        builder.HasIndex(libraryScanResult => libraryScanResult.Path)
            .IsUnique();
    }
}
