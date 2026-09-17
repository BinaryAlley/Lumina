#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;

/// <summary>
/// Validates the needed validation rules for <see cref="GetAlbumQuery"/>.
/// </summary>
public class GetAlbumQueryValidator : AbstractValidator<GetAlbumQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumQueryValidator"/> class.
    /// </summary>
    public GetAlbumQueryValidator()
    {
        RuleFor(query => query.AlbumId)
            .NotEmpty()
            .WithError(Errors.Music.AlbumIdCannotBeEmpty)
            .Must(albumId => albumId != Guid.Empty)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);
    }
}
