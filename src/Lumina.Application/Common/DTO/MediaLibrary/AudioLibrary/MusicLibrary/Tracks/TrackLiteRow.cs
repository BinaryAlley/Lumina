#region ========================================================================= USING =====================================================================================
using System;
#endregion

namespace Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Lightweight read model of a track, containing only the fields needed to display the track in a list.
/// </summary>
public sealed record TrackLiteRow
{
    /// <summary>
    /// Gets the Id of the track.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the title of the track.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the number of the track on its disc.
    /// </summary>
    public int TrackNumber { get; init; }

    /// <summary>
    /// Gets the number of the disc the track belongs to, if applicable.
    /// </summary>
    public int? DiscNumber { get; init; }
}
