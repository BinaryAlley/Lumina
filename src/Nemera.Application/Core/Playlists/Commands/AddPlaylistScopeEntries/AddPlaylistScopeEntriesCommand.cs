#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;

/// <summary>
/// Command to add every track of a music library, an artist or an album to a playlist, with the track Ids resolved by the server.
/// </summary>
/// <param name="PlaylistId">The Id of the playlist, taken from the route.</param>
/// <param name="LibraryId">The Id of the music library the scope belongs to.</param>
/// <param name="ArtistId">The Id of the artist the scope is limited to, if applicable.</param>
/// <param name="AlbumId">The Id of the album the scope is limited to, if applicable.</param>
[DebuggerDisplay("PlaylistId: {PlaylistId}; LibraryId: {LibraryId}; ArtistId: {ArtistId}; AlbumId: {AlbumId}")]
public record AddPlaylistScopeEntriesCommand(
    string? PlaylistId,
    Guid? LibraryId,
    Guid? ArtistId,
    Guid? AlbumId
) : ICommand;
