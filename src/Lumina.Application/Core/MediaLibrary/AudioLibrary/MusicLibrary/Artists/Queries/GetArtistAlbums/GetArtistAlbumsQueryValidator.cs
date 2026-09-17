#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbums;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistAlbumsQuery"/>.
/// </summary>
public class GetArtistAlbumsQueryValidator : AbstractValidator<GetArtistAlbumsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsQueryValidator"/> class.
    /// </summary>
    public GetArtistAlbumsQueryValidator()
    {
        RuleFor(query => query.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty)
            .Must(artistId => artistId != Guid.Empty)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);
    }
}
