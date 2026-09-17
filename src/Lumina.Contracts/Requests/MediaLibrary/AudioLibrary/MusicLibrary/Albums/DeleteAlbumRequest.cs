#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a request to delete an album by the specified Id.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album to delete.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record DeleteAlbumRequest(
    Guid ArtistId,
    Guid AlbumId
);
