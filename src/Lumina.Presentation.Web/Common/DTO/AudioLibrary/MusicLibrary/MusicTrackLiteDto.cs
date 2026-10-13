#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Data transfer object for a lightweight track element, containing only the properties needed by the client to display the tracks of an album.
/// </summary>
[DebuggerDisplay("Title: {Title}")]
public class MusicTrackLiteDto
{
    /// <summary>
    /// Gets or sets the Id of the track.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the track.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of the track on its disc.
    /// </summary>
    public int TrackNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of the disc the track belongs to, if applicable.
    /// </summary>
    public int? DiscNumber { get; set; }

    /// <summary>
    /// Gets or sets the duration of the track in seconds.
    /// </summary>
    public int DurationInSeconds { get; set; }
}
