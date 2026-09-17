#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracks;

/// <summary>
/// Query for getting all the tracks of an album.
/// </summary>
/// <param name="AlbumId">The unique identifier of the album whose tracks are retrieved.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumTracksQuery(
    Guid AlbumId
) : IQuery;
