#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Lightweight read model of an album, containing only the fields needed to display the album in a card-based grid or a list.
/// </summary>
public sealed record AlbumLiteRow
{
    /// <summary>
    /// Gets the Id of the album.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the album.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the number of tracks of the release.
    /// </summary>
    public int TotalTracks { get; init; }
}
