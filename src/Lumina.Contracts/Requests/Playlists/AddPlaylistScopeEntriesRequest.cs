#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Playlists;

/// <summary>
/// Represents a request to add every track of a music library, an artist or an album to a playlist, resolved on the server.
/// </summary>
/// <param name="LibraryId">The Id of the music library the scope belongs to. Required.</param>
/// <param name="ArtistId">The Id of the artist the scope is limited to. Optional.</param>
/// <param name="AlbumId">The Id of the album the scope is limited to. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId}; ArtistId: {ArtistId}; AlbumId: {AlbumId}")]
public record AddPlaylistScopeEntriesRequest(
    Guid LibraryId,
    Guid? ArtistId = null,
    Guid? AlbumId = null
);
