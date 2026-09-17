#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;

/// <summary>
/// Query for getting the lightweight read models of all the tracks of an album.
/// </summary>
/// <param name="AlbumId">The unique identifier of the album whose tracks are retrieved.</param>
/// <param name="CurrentPage">The page of results to retrieve.</param>
/// <param name="PerPage">The maximum number of tracks to retrieve per page.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumTracksLiteQuery(
    Guid AlbumId,
    int? CurrentPage,
    int? PerPage
) : IQuery;
