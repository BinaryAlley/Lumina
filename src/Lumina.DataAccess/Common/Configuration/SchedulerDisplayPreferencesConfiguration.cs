#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="SchedulerDisplayPreferencesEntity"/> entity.
/// </summary>
public class SchedulerDisplayPreferencesConfiguration : IEntityTypeConfiguration<SchedulerDisplayPreferencesEntity>
{
    /// <summary>
    /// Configures the <see cref="SchedulerDisplayPreferencesEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<SchedulerDisplayPreferencesEntity> builder)
    {
        builder.ToTable("SchedulerDisplayPreferences");
        builder.HasKey(preferences => preferences.Id);
        builder.Property(preferences => preferences.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(preferences => preferences.UserId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(preferences => preferences.JobTypeFilter)
            .HasConversion<string>()
            .HasColumnOrder(2);

        builder.Property(preferences => preferences.DisplayTimeSpan)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(preferences => preferences.DisplayTimeUnit)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(4);

        // Audit.
        builder.Property(preferences => preferences.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(preferences => preferences.CreatedBy)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(preferences => preferences.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(7);

        builder.Property(preferences => preferences.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        // One user has at most one set of scheduler page display preferences.
        builder.HasIndex(preferences => preferences.UserId)
            .IsUnique();
    }
}
