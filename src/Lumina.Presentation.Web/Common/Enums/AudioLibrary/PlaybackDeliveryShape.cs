#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the shapes in which delivered audio is addressed and seeked.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum PlaybackDeliveryShape
{
    /// <summary>
    /// The delivered media is the original file itself, addressed and seeked by byte range.
    /// </summary>
    ByteRangeSeekable,

    /// <summary>
    /// The delivered media is a sequence of fixed duration segments, addressed and seeked by time, so that finding a time never
    /// requires mapping a byte range to it.
    /// </summary>
    TimeAddressedSegments,
}
