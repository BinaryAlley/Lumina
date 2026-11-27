#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;

/// <summary>
/// Validates the needed validation rules for <see cref="GetDeletedAlbumsQuery"/>.
/// </summary>
public class GetDeletedAlbumsQueryValidator : AbstractValidator<GetDeletedAlbumsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedAlbumsQueryValidator"/> class.
    /// </summary>
    public GetDeletedAlbumsQueryValidator()
    {
        // Validates the identifier of the media library whose deleted albums are retrieved, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
