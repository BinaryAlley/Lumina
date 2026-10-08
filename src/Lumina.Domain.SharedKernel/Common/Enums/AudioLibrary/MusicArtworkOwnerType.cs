#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the type of the music library item that owns a piece of artwork.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicArtworkOwnerType
{
    /// <summary>
    /// The artwork belongs to an artist.
    /// </summary>
    Artist,

    /// <summary>
    /// The artwork belongs to an album.
    /// </summary>
    Album,

    /// <summary>
    /// The artwork belongs to a track.
    /// </summary>
    Track
}
