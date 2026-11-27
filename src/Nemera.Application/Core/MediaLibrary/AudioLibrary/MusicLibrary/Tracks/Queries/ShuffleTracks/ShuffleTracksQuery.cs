#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.ShuffleTracks;

/// <summary>
/// Query for getting a random selection of tracks from a media library, an artist or an album.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the tracks are sampled from, taken from the route.</param>
/// <param name="Count">The maximum number of tracks to return.</param>
/// <param name="ArtistId">The unique identifier of the artist whose tracks are sampled, if applicable.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are sampled, if applicable.</param>
/// <param name="ExcludedTrackIds">The tracks that must not be returned, if any.</param>
[DebuggerDisplay("LibraryId: {LibraryId} Count: {Count}")]
public record ShuffleTracksQuery(
    string? LibraryId,
    int Count,
    Guid? ArtistId,
    Guid? AlbumId,
    IReadOnlyList<Guid>? ExcludedTrackIds
) : IQuery;
