#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.Playlists;

/// <summary>
/// Enumeration for the kinds of media a playlist can hold.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum PlaylistType
{
    /// <summary>
    /// A playlist whose ordered entries reference music tracks.
    /// </summary>
    Music,
}
