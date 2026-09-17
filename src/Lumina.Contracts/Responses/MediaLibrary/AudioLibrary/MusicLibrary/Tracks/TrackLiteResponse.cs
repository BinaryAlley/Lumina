#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Represents a lightweight track response, containing only the fields needed by the client to display the tracks of an album.
/// </summary>
/// <param name="Id">The Id of the track.</param>
/// <param name="Title">The title of the track.</param>
/// <param name="TrackNumber">The number of the track on its disc.</param>
/// <param name="DiscNumber">The number of the disc the track belongs to, if applicable.</param>
[DebuggerDisplay("Title: {Title}")]
public record TrackLiteResponse(
    Guid Id,
    string Title,
    int TrackNumber,
    int? DiscNumber
);
