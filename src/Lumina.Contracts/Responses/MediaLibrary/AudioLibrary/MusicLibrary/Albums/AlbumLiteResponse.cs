#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Represents a lightweight album response, containing only the fields needed by the client for card-style navigation.
/// </summary>
/// <param name="Id">The Id of the album.</param>
/// <param name="Title">The title of the album.</param>
/// <param name="TotalTracks">The number of tracks of the release.</param>
[DebuggerDisplay("Title: {Title}")]
public record AlbumLiteResponse(
    Guid Id,
    string Title,
    int TotalTracks
);
