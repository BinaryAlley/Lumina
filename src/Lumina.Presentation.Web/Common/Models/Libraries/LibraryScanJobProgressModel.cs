#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Libraries;

/// <summary>
/// Represents a media library scan job progress model.
/// </summary>
[DebuggerDisplay("CompletedItems: {CompletedItems}; TotalItems: {TotalItems}")]
public class LibraryScanJobProgressModel
{
    /// <summary>
    /// Gets or sets the number of completed items of the media library scan job.
    /// </summary>
    public int CompletedItems { get; set; }

    /// <summary>
    /// Gets or sets the total number of items of the media library scan job.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the current operation being performed by the media library scan job.
    /// </summary>
    public string? CurrentOperation { get; set; }

    /// <summary>
    /// Gets or sets the ratio between the total number and the completed number of items, as a percentage.
    /// </summary>
    public decimal ProgressPercentage { get; set; }
}
