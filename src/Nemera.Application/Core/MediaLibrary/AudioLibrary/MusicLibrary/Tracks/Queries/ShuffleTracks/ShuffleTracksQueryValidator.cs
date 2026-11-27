#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;

/// <summary>
/// Validates the needed validation rules for <see cref="ShuffleTracksQuery"/>.
/// </summary>
public class ShuffleTracksQueryValidator : AbstractValidator<ShuffleTracksQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksQueryValidator"/> class.
    /// </summary>
    public ShuffleTracksQueryValidator()
    {
        // Validates the identifier of the media library whose tracks are sampled, taken from the route.
        RuleFor(query => query.LibraryId)
            .Must(libraryId => !string.IsNullOrEmpty(libraryId))
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => !string.IsNullOrEmpty(query.LibraryId))
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.Count)
            .Must(count => count > 0)
            .WithError(Errors.Music.ShuffleTrackCountMustBeGreaterThanZero);

        RuleFor(query => query.Count)
            .Must(count => count <= 500)
            .WithError(Errors.Music.ShuffleTrackCountMustBeAtMost500);

        // The optional scoping identifiers must be real identifiers when they are present.
        RuleFor(query => query.ArtistId)
            .Must(artistId => !artistId.HasValue || artistId.Value != Guid.Empty)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        RuleFor(query => query.AlbumId)
            .Must(albumId => !albumId.HasValue || albumId.Value != Guid.Empty)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);
    }
}
