#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;

/// <summary>
/// Validates the needed validation rules for <see cref="GetArtistsQuery"/>.
/// </summary>
public class GetArtistsQueryValidator : AbstractValidator<GetArtistsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistsQueryValidator"/> class.
    /// </summary>
    public GetArtistsQueryValidator()
    {
        // Validates the identifier of the media library whose artists are retrieved, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
