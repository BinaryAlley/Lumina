#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="UserSettingsEntity"/> entity.
/// </summary>
public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettingsEntity>
{
    /// <summary>
    /// Configures the <see cref="UserSettingsEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
    {
        builder.ToTable("UserSettings");
        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        // one user with one set of settings
        builder.HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<UserSettingsEntity>(settings => settings.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(settings => settings.UserId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(settings => settings.IsPaginationEnabled)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(settings => settings.ItemsPerPage)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(settings => settings.ShouldIgnoreThePrefixForAlphaPicker)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(settings => settings.ShouldAggregateMetadataWhenMissing)
            .IsRequired()
            .HasColumnOrder(10);

        builder.Property(settings => settings.ShouldAggregateArtworkWhenMissing)
            .IsRequired()
            .HasColumnOrder(13);

        builder.Property(settings => settings.ShouldRenderPdfAsImages)
            .IsRequired()
            .HasColumnOrder(11);

        builder.Property(settings => settings.ShouldPreserveBookStyles)
            .IsRequired()
            .HasColumnOrder(12);

        builder.Property(settings => settings.IsThemeCachingEnabled)
            .IsRequired()
            .HasColumnOrder(9);

        builder.Property(settings => settings.ShouldKeepDownloadingWhilePaused)
            .IsRequired()
            .HasColumnOrder(14);

        builder.Property(settings => settings.ShouldAutoplay)
            .IsRequired()
            .HasColumnOrder(15);

        builder.Property(settings => settings.IsGaplessPlaybackEnabled)
            .IsRequired()
            .HasColumnOrder(16);

        builder.Property(settings => settings.ReplayGainMode)
            .IsRequired()
            .HasColumnOrder(17);

        builder.Property(settings => settings.StreamingQuality)
            .IsRequired()
            .HasColumnOrder(18);

        builder.Property(settings => settings.VuMeterOrientation)
            .IsRequired()
            .HasColumnOrder(19);

        builder.Property(settings => settings.VuMeterSegmentCount)
            .IsRequired()
            .HasColumnOrder(20);

        builder.Property(settings => settings.VuMeterSegmentGap)
            .IsRequired()
            .HasColumnOrder(21);

        builder.Property(settings => settings.VuMeterMediumThreshold)
            .IsRequired()
            .HasColumnOrder(22);

        builder.Property(settings => settings.VuMeterHighThreshold)
            .IsRequired()
            .HasColumnOrder(23);

        builder.Property(settings => settings.VuMeterShowPeak)
            .IsRequired()
            .HasColumnOrder(24);

        builder.Property(settings => settings.VuMeterPeakHoldDurationInMilliseconds)
            .IsRequired()
            .HasColumnOrder(25);

        builder.Property(settings => settings.VuMeterReleasePerSecond)
            .IsRequired()
            .HasColumnOrder(26);

        builder.Property(settings => settings.VuMeterPeakReleasePerSecond)
            .IsRequired()
            .HasColumnOrder(27);

        builder.Property(settings => settings.Volume)
            .IsRequired()
            .HasColumnOrder(28);

        builder.Property(settings => settings.IsMuted)
            .IsRequired()
            .HasColumnOrder(29);

        // audit
        builder.Property(settings => settings.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(settings => settings.CreatedBy)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(settings => settings.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(7);

        builder.Property(settings => settings.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        // one user can have at most one set of settings
        builder.HasIndex(settings => settings.UserId)
            .IsUnique();
    }
}
