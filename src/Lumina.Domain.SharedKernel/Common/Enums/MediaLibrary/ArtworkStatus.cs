#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the status of the artwork enrichment of a media library item.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum ArtworkStatus
{
    /// <summary>
    /// The artwork of the media library item has not been resolved yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The artwork of the media library item has been successfully resolved.
    /// </summary>
    Enriched,

    /// <summary>
    /// The artwork of the media library item was searched for, and no artwork exists for it.
    /// </summary>
    NotAvailable,

    /// <summary>
    /// The artwork resolution of the media library item failed, because an error prevented the artwork from being retrieved.
    /// </summary>
    Failed
}
