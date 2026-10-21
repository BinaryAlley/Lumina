#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Lightweight read model of a track, containing only the fields needed to display the track in a list.
/// </summary>
/// <param name="Id">The Id of the track.</param>
/// <param name="Title">The title of the track.</param>
/// <param name="TrackNumber">The number of the track on its disc.</param>
/// <param name="DiscNumber">The number of the disc the track belongs to, if applicable.</param>
/// <param name="DurationInSeconds">The duration of the track in seconds.</param>
public sealed record TrackLiteRow(
    Guid Id,
    string Title,
    int TrackNumber,
    int? DiscNumber,
    int DurationInSeconds
);
