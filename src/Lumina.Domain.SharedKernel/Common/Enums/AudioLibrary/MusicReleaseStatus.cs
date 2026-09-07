#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;

/// <summary>
/// Enumeration for the status of a music release, describing the officiality of the release.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MusicReleaseStatus
{
    /// <summary>
    /// The release was distributed without the authorization of the artist or label.
    /// </summary>
    Bootleg,

    /// <summary>
    /// The release was announced but never distributed.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The release was expunged from the catalog of the artist or label.
    /// </summary>
    Expunged,

    /// <summary>
    /// The release was released through official channels.
    /// </summary>
    Official,

    /// <summary>
    /// The release was distributed for promotional purposes only.
    /// </summary>
    Promotion,

    /// <summary>
    /// The release is a pseudo-release, not an actual release by the artist.
    /// </summary>
    PseudoRelease,

    /// <summary>
    /// The release was distributed but later withdrawn from circulation.
    /// </summary>
    Withdrawn
}
