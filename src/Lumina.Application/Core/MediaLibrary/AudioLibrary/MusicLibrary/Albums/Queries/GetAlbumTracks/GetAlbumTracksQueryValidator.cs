#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;

/// <summary>
/// Validates the needed validation rules for <see cref="GetAlbumTracksQuery"/>.
/// </summary>
public class GetAlbumTracksQueryValidator : AbstractValidator<GetAlbumTracksQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksQueryValidator"/> class.
    /// </summary>
    public GetAlbumTracksQueryValidator()
    {
        RuleFor(query => query.AlbumId)
            .NotEmpty()
            .WithError(Errors.Music.AlbumIdCannotBeEmpty)
            .Must(albumId => albumId != Guid.Empty)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);
    }
}
