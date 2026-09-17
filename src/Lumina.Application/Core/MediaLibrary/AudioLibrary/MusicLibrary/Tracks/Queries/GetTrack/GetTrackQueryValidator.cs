#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTrackQuery"/>.
/// </summary>
public class GetTrackQueryValidator : AbstractValidator<GetTrackQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackQueryValidator"/> class.
    /// </summary>
    public GetTrackQueryValidator()
    {
        RuleFor(query => query.TrackId)
            .NotEmpty()
            .WithError(Errors.Music.TrackIdCannotBeEmpty)
            .Must(trackId => trackId != Guid.Empty)
            .WithError(Errors.Music.TrackIdCannotBeEmpty);
    }
}
