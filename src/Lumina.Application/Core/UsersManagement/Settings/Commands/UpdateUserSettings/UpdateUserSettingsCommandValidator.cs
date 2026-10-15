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
    }
}
