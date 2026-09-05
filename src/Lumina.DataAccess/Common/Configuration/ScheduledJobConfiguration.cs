#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="ScheduledJobEntity"/> entity.
/// </summary>
public class ScheduledJobConfiguration : IEntityTypeConfiguration<ScheduledJobEntity>
{
    /// <summary>
    /// Configures the <see cref="ScheduledJobEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<ScheduledJobEntity> builder)
    {
        builder.ToTable("ScheduledJobs");
        builder.HasKey(scheduledJob => scheduledJob.Id);
        builder.Property(scheduledJob => scheduledJob.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(scheduledJob => scheduledJob.Name)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(scheduledJob => scheduledJob.TaskType)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(2);

        builder.Property(scheduledJob => scheduledJob.ScheduleType)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(3);

        builder.Property(scheduledJob => scheduledJob.IntervalMinutes)
            .HasColumnOrder(4);

        builder.Property(scheduledJob => scheduledJob.Hour)
            .HasColumnOrder(5);

        builder.Property(scheduledJob => scheduledJob.Minute)
            .HasColumnOrder(6);

        builder.Property(scheduledJob => scheduledJob.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(7);

        builder.Property(scheduledJob => scheduledJob.OwnerUserId)
            .IsRequired()
            .HasColumnOrder(8);

        builder.Property(scheduledJob => scheduledJob.LastStartedOnUtc)
            .HasColumnOrder(9);

        builder.Property(scheduledJob => scheduledJob.LastCompletedOnUtc)
            .HasColumnOrder(10);

        // Audit.
        builder.Property(scheduledJob => scheduledJob.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(11);

        builder.Property(scheduledJob => scheduledJob.CreatedBy)
            .IsRequired()
            .HasColumnOrder(12);

        builder.Property(scheduledJob => scheduledJob.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(13);

        builder.Property(scheduledJob => scheduledJob.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(14);

        // Enables fast lookup of the scheduled jobs whose execution cycle is active, at scheduler startup.
        builder.HasIndex(scheduledJob => scheduledJob.Status);
    }
}
