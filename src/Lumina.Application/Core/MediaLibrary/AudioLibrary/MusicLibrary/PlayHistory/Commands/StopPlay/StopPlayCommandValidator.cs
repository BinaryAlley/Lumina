#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.StopPlay;

/// <summary>
/// Validates the needed validation rules for <see cref="StopPlayCommand"/>.
/// </summary>
public class StopPlayCommandValidator : AbstractValidator<StopPlayCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StopPlayCommandValidator"/> class.
    /// </summary>
    public StopPlayCommandValidator()
    {
        RuleFor(command => command.PlayRecordId)
            .NotEmpty()
            .WithError(Errors.MusicPlayHistory.PlayRecordIdCannotBeEmpty);

        RuleFor(command => command.PlayRecordId)
            .Must(playRecordId => Guid.TryParse(playRecordId, out Guid parsedPlayRecordId) && parsedPlayRecordId != Guid.Empty)
            .When(command => command.PlayRecordId is not null && command.PlayRecordId.Length > 0)
            .WithError(Errors.MusicPlayHistory.PlayRecordIdCannotBeEmpty);

        RuleFor(command => command.PositionInSeconds)
            .NotNull()
            .WithError(Errors.MusicPlayHistory.PlaybackPositionCannotBeNull);

        RuleFor(command => command.PositionInSeconds)
            .Must(position => position is null || (double.IsFinite(position.Value) && position.Value >= 0))
            .WithError(Errors.MusicPlayHistory.PlaybackPositionMustBeNonNegative);

        RuleFor(command => command.PositionInSeconds)
            .Must(position => position is null || position.Value <= TimeSpan.MaxValue.TotalSeconds)
            .WithError(Errors.MusicPlayHistory.PlaybackPositionIsOutOfRange);

        RuleFor(command => command.ListenedDurationInSeconds)
            .NotNull()
            .WithError(Errors.MusicPlayHistory.ListenedDurationCannotBeNull);

        RuleFor(command => command.ListenedDurationInSeconds)
            .Must(listenedDuration => listenedDuration is null || (double.IsFinite(listenedDuration.Value) && listenedDuration.Value >= 0))
            .WithError(Errors.MusicPlayHistory.ListenedDurationMustBeNonNegative);

        RuleFor(command => command.ListenedDurationInSeconds)
            .Must(listenedDuration => listenedDuration is null || listenedDuration.Value <= TimeSpan.MaxValue.TotalSeconds)
            .WithError(Errors.MusicPlayHistory.ListenedDurationIsOutOfRange);
    }
}
