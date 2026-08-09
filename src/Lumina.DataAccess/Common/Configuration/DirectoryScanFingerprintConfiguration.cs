#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="DirectoryScanFingerprintEntity"/> entity.
/// </summary>
public class DirectoryScanFingerprintConfiguration : IEntityTypeConfiguration<DirectoryScanFingerprintEntity>
{
    /// <summary>
    /// Configures the <see cref="DirectoryScanFingerprintEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<DirectoryScanFingerprintEntity> builder)
    {
        builder.ToTable("DirectoryScanFingerprints");
        // composite key, so that each directory scan fingerprint is uniquely identified within its library by its path
        builder.HasKey(directoryScanFingerprint => new { directoryScanFingerprint.LibraryId, directoryScanFingerprint.Path });

        // one library with many directory scan fingerprints
        builder.HasOne<LibraryEntity>()
            .WithMany()
            .HasForeignKey(directoryScanFingerprint => directoryScanFingerprint.LibraryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(directoryScanFingerprint => directoryScanFingerprint.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(directoryScanFingerprint => directoryScanFingerprint.Path)
            .HasMaxLength(1024)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(directoryScanFingerprint => directoryScanFingerprint.LastWriteTimeUtc)
            .IsRequired()
            .HasColumnOrder(2);

        // enables fast lookups of the directory scan fingerprints of a library
        builder.HasIndex(directoryScanFingerprint => directoryScanFingerprint.LibraryId);
    }
}
