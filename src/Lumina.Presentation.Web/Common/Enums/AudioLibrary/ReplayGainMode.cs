#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the ways ReplayGain normalization is applied to the tracks a user plays.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum ReplayGainMode
{
    /// <summary>
    /// ReplayGain is not applied. Tracks play at the loudness at which they were stored.
    /// </summary>
    Off,

    /// <summary>
    /// Each track is normalized using its own ReplayGain track gain, so tracks from different albums play at a similar loudness.
    /// </summary>
    Track,

    /// <summary>
    /// Each track is normalized using the ReplayGain album gain of the album it belongs to, so an album keeps its relative loudness
    /// while albums play at a similar loudness to each other.
    /// </summary>
    Album,
}
