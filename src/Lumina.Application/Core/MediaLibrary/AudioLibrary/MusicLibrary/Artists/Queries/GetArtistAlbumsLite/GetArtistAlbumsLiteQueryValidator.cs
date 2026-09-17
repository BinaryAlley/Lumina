#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistAlbumsLiteQuery"/>.
/// </summary>
public class GetArtistAlbumsLiteQueryValidator : AbstractValidator<GetArtistAlbumsLiteQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsLiteQueryValidator"/> class.
    /// </summary>
    public GetArtistAlbumsLiteQueryValidator()
    {
        RuleFor(query => query.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty)
            .Must(artistId => artistId != Guid.Empty)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);
    }
}
