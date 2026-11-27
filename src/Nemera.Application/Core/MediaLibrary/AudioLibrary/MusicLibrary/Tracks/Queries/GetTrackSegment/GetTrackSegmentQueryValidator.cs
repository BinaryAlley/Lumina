#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Errors;
using System;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTrackSegmentQuery"/>.
/// </summary>
public class GetTrackSegmentQueryValidator : AbstractValidator<GetTrackSegmentQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentQueryValidator"/> class.
    /// </summary>
    public GetTrackSegmentQueryValidator()
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

        // Validates the identifier of the track whose normalized audio is streamed, taken from the route.
        RuleFor(query => query.TrackId)
            .NotEmpty()
            .WithError(Errors.Music.TrackIdCannotBeEmpty);

        RuleFor(query => query.TrackId)
            .Must(trackId => Guid.TryParse(trackId, out Guid parsedTrackId) && parsedTrackId != Guid.Empty)
            .When(query => query.TrackId is not null && query.TrackId.Length > 0)
            .WithError(Errors.Music.TrackIdCannotBeEmpty);

        // Validates the opaque delivery handle the segment belongs to, taken from the route.
        RuleFor(query => query.DeliveryId)
            .NotEmpty()
            .WithError(Errors.Playback.DeliveryIdCannotBeEmpty);

        RuleFor(query => query.DeliveryId)
            .Must(PlaybackDeliveryId.IsValid)
            .When(query => query.DeliveryId is not null && query.DeliveryId.Length > 0)
            .WithError(Errors.Playback.DeliveryIdIsInvalid);

        // Validates the index of the segment of the normalized audio, taken from the route.
        RuleFor(query => query.SegmentIndex)
            .NotEmpty()
            .WithError(Errors.Playback.SegmentIndexCannotBeEmpty);

        RuleFor(query => query.SegmentIndex)
            .Must(segmentIndex => int.TryParse(segmentIndex, out int parsedSegmentIndex) && parsedSegmentIndex >= 0)
            .When(query => query.SegmentIndex is not null && query.SegmentIndex.Length > 0)
            .WithError(Errors.Playback.SegmentIndexMustBeValid);
    }
}
