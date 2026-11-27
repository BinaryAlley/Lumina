#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a lightweight album response, containing only the fields needed by the client for card-style navigation.
/// </summary>
/// <param name="Id">The Id of the album.</param>
/// <param name="Title">The title of the album.</param>
/// <param name="TotalTracks">The number of tracks of the release.</param>
/// <param name="CoverPath">The path of the image representing the album, if available.</param>
/// <param name="ReleaseYear">The release year of the album (the re-release year, if available, or the original release year), if known.</param>
/// <param name="ReleaseTypes">The set of types of the release.</param>
[DebuggerDisplay("Title: {Title}")]
public record AlbumLiteResponse(
    Guid Id,
    string Title,
    int TotalTracks,
    string? CoverPath,
    int? ReleaseYear,
    List<MusicReleaseType>? ReleaseTypes
);
