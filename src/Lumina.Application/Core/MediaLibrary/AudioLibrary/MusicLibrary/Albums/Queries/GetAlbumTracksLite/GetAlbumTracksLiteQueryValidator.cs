#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;

/// <summary>
/// Validates the needed validation rules for <see cref="GetAlbumTracksLiteQuery"/>.
/// </summary>
public class GetAlbumTracksLiteQueryValidator : AbstractValidator<GetAlbumTracksLiteQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksLiteQueryValidator"/> class.
    /// </summary>
    public GetAlbumTracksLiteQueryValidator()
    {
        RuleFor(query => query.AlbumId)
            .NotEmpty()
            .WithError(Errors.Music.AlbumIdCannotBeEmpty)
            .Must(albumId => albumId != Guid.Empty)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);
    }
}
