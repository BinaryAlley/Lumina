#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Lightweight read model of an album, containing only the fields needed to display the album in a card-based grid or a list.
/// </summary>
/// <param name="Id">The Id of the album.</param>
/// <param name="Title">The title of the album.</param>
/// <param name="TotalTracks">The number of tracks of the release.</param>
/// <param name="CoverPath">The path of the image representing the album, if available.</param>
/// <param name="ReleaseYear">The release year of the album (the re-release year, if available, or the original release year), if known.</param>
/// <param name="ReleaseTypes">The set of types of the release.</param>
public sealed record AlbumLiteRow(
    Guid Id,
    string Title,
    int TotalTracks,
    string? CoverPath,
    int? ReleaseYear,
    List<MusicReleaseType>? ReleaseTypes
);
