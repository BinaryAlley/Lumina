#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;

/// <summary>
/// Query for getting all the tracks of an album.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the album belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album whose tracks are retrieved, taken from the route.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumTracksQuery(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId
) : IQuery;
