#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTrackQuery"/>.
/// </summary>
public class GetTrackQueryValidator : AbstractValidator<GetTrackQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackQueryValidator"/> class.
    /// </summary>
    public GetTrackQueryValidator()
    {
        // Validates the identifier of the media library the track belongs to, taken from the route.
        RuleFor(query => query.LibraryId)
            .NotEmpty()
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        RuleFor(query => query.LibraryId)
            .Must(libraryId => Guid.TryParse(libraryId, out Guid parsedLibraryId) && parsedLibraryId != Guid.Empty)
            .When(query => query.LibraryId is not null && query.LibraryId.Length > 0)
            .WithError(Errors.Library.LibraryIdCannotBeEmpty);

        // Validates the identifier of the artist the album of the track belongs to, taken from the route.
        RuleFor(query => query.ArtistId)
            .NotEmpty()
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        RuleFor(query => query.ArtistId)
            .Must(artistId => Guid.TryParse(artistId, out Guid parsedArtistId) && parsedArtistId != Guid.Empty)
            .When(query => query.ArtistId is not null && query.ArtistId.Length > 0)
            .WithError(Errors.Music.ArtistIdCannotBeEmpty);

        // Validates the identifier of the album the track belongs to, taken from the route.
        RuleFor(query => query.AlbumId)
            .NotEmpty()
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);

        RuleFor(query => query.AlbumId)
            .Must(albumId => Guid.TryParse(albumId, out Guid parsedAlbumId) && parsedAlbumId != Guid.Empty)
            .When(query => query.AlbumId is not null && query.AlbumId.Length > 0)
            .WithError(Errors.Music.AlbumIdCannotBeEmpty);

        // Validates the identifier of the track to get, taken from the route.
        RuleFor(query => query.TrackId)
            .NotEmpty()
            .WithError(Errors.Music.TrackIdCannotBeEmpty);

        RuleFor(query => query.TrackId)
            .Must(trackId => Guid.TryParse(trackId, out Guid parsedTrackId) && parsedTrackId != Guid.Empty)
            .When(query => query.TrackId is not null && query.TrackId.Length > 0)
            .WithError(Errors.Music.TrackIdCannotBeEmpty);
    }
}
