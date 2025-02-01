#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Libraries;

/// <summary>
/// Represents a media library scan response.
/// </summary>
[DebuggerDisplay("ScanId: {ScanId} LibraryId: {LibraryId}")]
public class ScanLibraryModel
{
    /// <summary>
    /// Gets or sets the Id of the media library scan.
    /// </summary>
    public Guid? ScanId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the scanned media library.
    /// </summary>
    public Guid? LibraryId { get; set; }
}
