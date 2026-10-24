#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="MediaResumePointEntity"/> entity.
/// </summary>
public class MediaResumePointConfiguration : IEntityTypeConfiguration<MediaResumePointEntity>
{
    /// <summary>
    /// Configures the <see cref="MediaResumePointEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<MediaResumePointEntity> builder)
    {
        builder.ToTable("MediaResumePoints");
        builder.HasKey(resumePoint => resumePoint.Id);
        builder.Property(resumePoint => resumePoint.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        // A resume point belongs to the user that owns it, and it is removed with that user.
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(resumePoint => resumePoint.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(resumePoint => resumePoint.UserId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(resumePoint => resumePoint.LibraryId)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(resumePoint => resumePoint.MediaKind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(3);

        builder.Property(resumePoint => resumePoint.MediaItemId)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(resumePoint => resumePoint.LastPositionKind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(5);

        builder.Property(resumePoint => resumePoint.LastPositionTimeOffsetMilliseconds)
            .HasColumnOrder(6);

        builder.Property(resumePoint => resumePoint.LastPositionLocation)
            .HasMaxLength(4096)
            .HasColumnOrder(7);

        builder.Property(resumePoint => resumePoint.LastProgressFraction)
            .IsRequired()
            .HasColumnOrder(8);

        builder.Property(resumePoint => resumePoint.LastConsumedOnUtc)
            .IsRequired()
            .HasColumnOrder(9);

        // The version is the optimistic concurrency token: every update increments it, and an update whose loaded version no longer matches
        // the stored one affects no row and fails, instead of reverting a resume point that another report already advanced.
        builder.Property(resumePoint => resumePoint.Version)
            .IsRequired()
            .IsConcurrencyToken()
            .HasColumnOrder(10);

        // Audit.
        builder.Property(resumePoint => resumePoint.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(11);

        builder.Property(resumePoint => resumePoint.CreatedBy)
            .IsRequired()
            .HasColumnOrder(12);

        builder.Property(resumePoint => resumePoint.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(13);

        builder.Property(resumePoint => resumePoint.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(14);

        // The referenced media item is kept by kind and Id only, without a foreign key: one table serves every media kind, so no single table
        // can be referenced, and a resume point survives the deletion of the item it references.
        // The unique index makes the storage medium the owner of the "one resume point per user and item" invariant.
        builder.HasIndex(resumePoint => new { resumePoint.UserId, resumePoint.MediaKind, resumePoint.MediaItemId })
            .IsUnique();
    }
}
