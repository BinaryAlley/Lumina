#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteTrackCommand"/>.
/// </summary>
public class DeleteTrackCommandValidator : AbstractValidator<DeleteTrackCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTrackCommandValidator"/> class.
    /// </summary>
    public DeleteTrackCommandValidator()
    {
        RuleFor(command => command.TrackId)
            .NotEmpty()
            .WithError(Errors.Music.TrackIdCannotBeEmpty)
            .Must(trackId => trackId != Guid.Empty)
            .WithError(Errors.Music.TrackIdCannotBeEmpty);
    }
}
