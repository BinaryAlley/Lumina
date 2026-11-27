#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Enums.Playlists;

/// <summary>
/// Enumeration for the kinds of media item an ordered playlist entry can reference.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum PlaylistMediaItemType
{
    /// <summary>
    /// An entry that references a music track.
    /// </summary>
    MusicTrack,

    /// <summary>
    /// An entry that references a video file.
    /// </summary>
    VideoFile,
}
