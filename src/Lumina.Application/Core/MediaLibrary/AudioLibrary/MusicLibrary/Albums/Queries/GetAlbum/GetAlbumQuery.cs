#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbum;

/// <summary>
/// Query for getting an album by its Id.
/// </summary>
/// <param name="AlbumId">The unique identifier of the album to get.</param>
[DebuggerDisplay("AlbumId: {AlbumId}")]
public record GetAlbumQuery(
    Guid AlbumId
) : IQuery;
