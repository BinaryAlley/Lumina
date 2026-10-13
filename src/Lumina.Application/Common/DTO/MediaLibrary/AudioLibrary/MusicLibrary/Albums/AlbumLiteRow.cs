#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Gets the path of the image representing the album, if available.
    /// </summary>
    public string? CoverPath { get; init; }

    /// <summary>
    /// Gets the release year of the album (the re-release year, if available, or the original release year), if known.
    /// </summary>
    public int? ReleaseYear { get; init; }

    /// <summary>
    /// Gets the set of types of the release.
    /// </summary>
    public List<MusicReleaseType>? ReleaseTypes { get; init; }
}
