#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdateUserSettingsCommand"/>.
/// </summary>
public class UpdateUserSettingsCommandValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsCommandValidator"/> class.
    /// </summary>
    public UpdateUserSettingsCommandValidator()
    {
        RuleFor(command => command.ItemsPerPage)
            .GreaterThan(0)
            .WithError(Errors.UserSettings.ItemsPerPageMustBeGreaterThanZero);

        RuleFor(command => command.ShouldKeepDownloadingWhilePaused)
            .NotNull()
            .WithError(Errors.UserSettings.ShouldKeepDownloadingWhilePausedCannotBeNull);

        RuleFor(command => command.ShouldAutoplay)
            .NotNull()
            .WithError(Errors.UserSettings.ShouldAutoplayCannotBeNull);

        RuleFor(command => command.IsGaplessPlaybackEnabled)
            .NotNull()
            .WithError(Errors.UserSettings.IsGaplessPlaybackEnabledCannotBeNull);

        RuleFor(command => command.ReplayGainMode)
            .NotNull()
            .WithError(Errors.UserSettings.ReplayGainModeCannotBeNull);

        RuleFor(command => command.ReplayGainMode)
            .IsInEnum()
            .WithError(Errors.UserSettings.InvalidReplayGainMode)
            .When(command => command.ReplayGainMode is not null);

        RuleFor(command => command.StreamingQuality)
            .NotNull()
            .WithError(Errors.UserSettings.StreamingQualityCannotBeNull);

        RuleFor(command => command.StreamingQuality)
            .IsInEnum()
            .WithError(Errors.UserSettings.InvalidStreamingQuality)
            .When(command => command.StreamingQuality is not null);

        RuleFor(command => command.VuMeterOrientation)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterOrientationCannotBeNull);

        RuleFor(command => command.VuMeterOrientation)
            .IsInEnum()
            .WithError(Errors.UserSettings.InvalidVuMeterOrientation)
            .When(command => command.VuMeterOrientation is not null);

        RuleFor(command => command.VuMeterSegmentCount)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterSegmentCountCannotBeNull);

        RuleFor(command => command.VuMeterSegmentCount)
            .InclusiveBetween(1, 128)
            .WithError(Errors.UserSettings.VuMeterSegmentCountMustBeBetweenOneAndOneHundredTwentyEight);

        RuleFor(command => command.VuMeterSegmentGap)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterSegmentGapCannotBeNull);

        RuleFor(command => command.VuMeterSegmentGap)
            .InclusiveBetween(0, 32)
            .WithError(Errors.UserSettings.VuMeterSegmentGapMustBeBetweenZeroAndThirtyTwo);

        RuleFor(command => command.VuMeterMediumThreshold)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterMediumThresholdCannotBeNull);

        RuleFor(command => command.VuMeterMediumThreshold)
            .InclusiveBetween(0d, 1d)
            .WithError(Errors.UserSettings.VuMeterMediumThresholdMustBeBetweenZeroAndOne);

        RuleFor(command => command.VuMeterHighThreshold)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterHighThresholdCannotBeNull);

        RuleFor(command => command.VuMeterHighThreshold)
            .InclusiveBetween(0d, 1d)
            .WithError(Errors.UserSettings.VuMeterHighThresholdMustBeBetweenZeroAndOne);

        RuleFor(command => command.VuMeterHighThreshold)
            .GreaterThanOrEqualTo(command => command.VuMeterMediumThreshold)
            .WithError(Errors.UserSettings.VuMeterHighThresholdMustBeGreaterThanOrEqualToMediumThreshold);

        RuleFor(command => command.VuMeterShowPeak)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterShowPeakCannotBeNull);

        RuleFor(command => command.VuMeterPeakHoldDurationInMilliseconds)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterPeakHoldDurationCannotBeNull);

        RuleFor(command => command.VuMeterPeakHoldDurationInMilliseconds)
            .InclusiveBetween(0, 10000)
            .WithError(Errors.UserSettings.VuMeterPeakHoldDurationMustBeBetweenZeroAndTenThousandMilliseconds);

        RuleFor(command => command.VuMeterReleasePerSecond)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterReleasePerSecondCannotBeNull);

        RuleFor(command => command.VuMeterReleasePerSecond)
            .InclusiveBetween(0d, 100d)
            .WithError(Errors.UserSettings.VuMeterReleasePerSecondMustBeBetweenZeroAndOneHundred);

        RuleFor(command => command.VuMeterPeakReleasePerSecond)
            .NotNull()
            .WithError(Errors.UserSettings.VuMeterPeakReleasePerSecondCannotBeNull);

        RuleFor(command => command.VuMeterPeakReleasePerSecond)
            .InclusiveBetween(0d, 100d)
            .WithError(Errors.UserSettings.VuMeterPeakReleasePerSecondMustBeBetweenZeroAndOneHundred);
    }
}
