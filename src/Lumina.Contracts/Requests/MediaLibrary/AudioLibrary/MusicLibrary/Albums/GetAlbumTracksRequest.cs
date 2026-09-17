#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request to get the tracks of an album.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are retrieved.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumTracksRequest(
    Guid ArtistId,
    Guid AlbumId
);
