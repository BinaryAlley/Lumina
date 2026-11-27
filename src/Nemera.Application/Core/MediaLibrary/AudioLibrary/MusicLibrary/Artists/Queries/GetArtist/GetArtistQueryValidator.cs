#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtist;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistQuery"/>.
/// </summary>
public class GetArtistQueryValidator : AbstractValidator<GetArtistQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistQueryValidator"/> class.
    /// </summary>
    public GetArtistQueryValidator()
    {
        // Validates the identifier of the media library the artist belongs to, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the artist to get, taken from the route.
        RuleFor(query => query.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        RuleFor(query => query.ArtistId)
            .Must(artistId => Guid.TryParse(artistId, out Guid parsedArtistId) && parsedArtistId != Guid.Empty)
            .When(query => query.ArtistId is not null && query.ArtistId.Length > 0)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);
    }
}
