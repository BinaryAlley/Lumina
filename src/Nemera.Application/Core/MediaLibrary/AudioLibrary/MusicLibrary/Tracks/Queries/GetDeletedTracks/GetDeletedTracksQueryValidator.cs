#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
using System;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;

/// <summary>
/// Validates the needed validation rules for <see cref="GetDeletedTracksQuery"/>.
/// </summary>
public class GetDeletedTracksQueryValidator : AbstractValidator<GetDeletedTracksQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedTracksQueryValidator"/> class.
    /// </summary>
    public GetDeletedTracksQueryValidator()
    {
        // Validates the identifier of the media library whose deleted tracks are retrieved, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);
    }
}
