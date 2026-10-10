#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;

/// <summary>
/// Enumeration for the status of the metadata enrichment of a media library item.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public enum MetadataStatus
{
    /// <summary>
    /// The metadata of the media library item has not been enriched yet.
    /// </summary>
    Pending,

    /// <summary>
    /// The metadata of the media library item has been successfully enriched.
    /// </summary>
    Enriched,

    /// <summary>
    /// The metadata enrichment of the media library item failed.
    /// </summary>
    Failed
}
