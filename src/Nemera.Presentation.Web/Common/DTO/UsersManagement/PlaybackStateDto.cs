#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.DTO.UsersManagement;

/// <summary>
/// Data transfer object for the playback state a user last reported to the player. The player writes these values automatically, and they are not part of the settings the user edits.
/// </summary>
[DebuggerDisplay("Volume: {Volume}; IsMuted: {IsMuted}")]
public class PlaybackStateDto
{
    /// <summary>
    /// Gets or sets the last volume the player reported, as a value between 0 and 1.
    /// </summary>
    public double? Volume { get; set; }

    /// <summary>
    /// Gets or sets the last mute state the player reported.
    /// </summary>
    public bool? IsMuted { get; set; }
}
