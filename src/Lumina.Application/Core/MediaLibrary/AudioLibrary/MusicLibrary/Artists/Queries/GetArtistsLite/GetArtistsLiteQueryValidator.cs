#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistsLiteQuery"/>.
/// </summary>
public class GetArtistsLiteQueryValidator : AbstractValidator<GetArtistsLiteQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsLiteQueryValidator"/> class.
    /// </summary>
    public GetArtistsLiteQueryValidator()
    {
        // Validates the identifier of the media library whose artists are retrieved, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // The alpha key must be exactly one of the three picker bucket kinds, so that the filter specification can
        // derive the key of a name unambiguously: no filter, a number bucket, a symbol bucket, or a single ASCII letter.
        RuleFor(query => query.FilterAlphaKey)
            .Must(alphaKey => alphaKey is null
                || alphaKey == LibraryItemAlphaKeys.NUMBER
                || alphaKey == LibraryItemAlphaKeys.SYMBOL
                || (alphaKey.Length == 1 && char.IsAsciiLetter(alphaKey[0])))
            .WithError(Errors.Library.InvalidFilterAlphaKey);
    }
}
