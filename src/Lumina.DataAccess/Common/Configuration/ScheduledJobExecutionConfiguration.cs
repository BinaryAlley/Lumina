#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="ScheduledJobExecutionEntity"/> entity.
/// </summary>
public class ScheduledJobExecutionConfiguration : IEntityTypeConfiguration<ScheduledJobExecutionEntity>
{
    /// <summary>
    /// Configures the <see cref="ScheduledJobExecutionEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<ScheduledJobExecutionEntity> builder)
    {
        builder.ToTable("ScheduledJobExecutions");
        builder.HasKey(execution => execution.Id);
        builder.Property(execution => execution.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(execution => execution.ScheduledJobId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(execution => execution.TaskType)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(2);

        builder.Property(execution => execution.IsCycleRun)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(execution => execution.StartedOnUtc)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(execution => execution.CompletedOnUtc)
            .HasColumnOrder(5);

        builder.Property(execution => execution.WasCycleActive)
            .IsRequired()
            .HasColumnOrder(6);

        // Audit.
        builder.Property(execution => execution.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(7);

        builder.Property(execution => execution.CreatedBy)
            .IsRequired()
            .HasColumnOrder(8);

        builder.Property(execution => execution.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(9);

        builder.Property(execution => execution.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(10);

        // Enables fast queries of the execution history of scheduled jobs by their time range, and the cleanup of the executions of a removed scheduled job.
        builder.HasIndex(execution => new { execution.StartedOnUtc, execution.ScheduledJobId });
        builder.HasIndex(execution => execution.ScheduledJobId);
    }
}
