#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;

/// <summary>
/// Query for getting an album by its Id.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the album belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist the album belongs to, taken from the route.</param>
/// <param name="AlbumId">The unique identifier of the album to get, taken from the route.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumQuery(
    string? LibraryId,
    string? ArtistId,
    string? AlbumId
) : IQuery;
