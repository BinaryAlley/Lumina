#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
#endregion

namespace Nemera.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;

/// <summary>
/// Validates the needed validation rules for <see cref="UpdatePlaybackStateCommand"/>.
/// </summary>
public class UpdatePlaybackStateCommandValidator : AbstractValidator<UpdatePlaybackStateCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateCommandValidator"/> class.
    /// </summary>
    public UpdatePlaybackStateCommandValidator()
    {
        RuleFor(command => command.Volume)
            .NotNull()
            .WithError(Errors.UserSettings.VolumeCannotBeNull);

        RuleFor(command => command.Volume)
            .Must(volume => volume is null || (volume.Value >= 0.0 && volume.Value <= 1.0))
            .WithError(Errors.UserSettings.VolumeMustBeBetweenZeroAndOne);

        RuleFor(command => command.IsMuted)
            .NotNull()
            .WithError(Errors.UserSettings.IsMutedCannotBeNull);
    }
}
