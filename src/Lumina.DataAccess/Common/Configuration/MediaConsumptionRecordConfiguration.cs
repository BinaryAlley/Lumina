#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="MediaConsumptionRecordEntity"/> entity.
/// </summary>
public class MediaConsumptionRecordConfiguration : IEntityTypeConfiguration<MediaConsumptionRecordEntity>
{
    /// <summary>
    /// Configures the <see cref="MediaConsumptionRecordEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<MediaConsumptionRecordEntity> builder)
    {
        builder.ToTable("MediaConsumptionRecords");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        // A consumption record belongs to the user that started it, and it is removed with that user.
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(record => record.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(record => record.UserId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(record => record.LibraryId)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(record => record.MediaItemId)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(record => record.MediaKind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(4);

        builder.Property(record => record.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(5);

        builder.Property(record => record.StartedOnUtc)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(record => record.LastActivityOnUtc)
            .IsRequired()
            .HasColumnOrder(7);

        builder.Property(record => record.EndedOnUtc)
            .HasColumnOrder(8);

        builder.Property(record => record.ConsumedDurationMilliseconds)
            .IsRequired()
            .HasColumnOrder(9);

        builder.Property(record => record.LastPositionKind)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false)
            .HasColumnOrder(10);

        builder.Property(record => record.LastPositionTimeOffsetMilliseconds)
            .HasColumnOrder(11);

        builder.Property(record => record.LastPositionLocation)
            .HasMaxLength(4096)
            .HasColumnOrder(12);

        builder.Property(record => record.LastProgressFraction)
            .IsRequired()
            .HasColumnOrder(13);

        builder.Property(record => record.ConsecutivePagesRead)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnOrder(14);

        // The version is the optimistic concurrency token: every update increments it, and an update whose loaded version no longer matches
        // the stored one affects no row and fails, instead of reverting a session that another report already completed or stopped.
        builder.Property(record => record.Version)
            .IsRequired()
            .IsConcurrencyToken()
            .HasColumnOrder(15);

        // Audit.
        builder.Property(record => record.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(16);

        builder.Property(record => record.CreatedBy)
            .IsRequired()
            .HasColumnOrder(17);

        builder.Property(record => record.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(18);

        builder.Property(record => record.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(19);

        // The referenced media item is kept by kind and Id only, without a foreign key: one table serves every media kind, so no single table
        // can be referenced, and a session survives the deletion of the item it references.
        // Enables listing the history of a user from the most recent activity, optionally restricted to one media kind and item.
        builder.HasIndex(record => new { record.UserId, record.MediaKind, record.MediaItemId, record.LastActivityOnUtc });

        // Enables the statistics of a user restricted by media kind and date range.
        builder.HasIndex(record => new { record.UserId, record.MediaKind, record.StartedOnUtc });
    }
}
