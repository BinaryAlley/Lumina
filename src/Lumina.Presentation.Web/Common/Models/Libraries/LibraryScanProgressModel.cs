#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Models.Libraries;

/// <summary>
/// Represents a media library scan progress model.
/// </summary>
[DebuggerDisplay("ScanId: {ScanId}; UserId: {UserId}; LibraryId: {LibraryId}")]
public class LibraryScanProgressModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the media library scan.
    /// </summary>
    public Guid ScanId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user initiating this media library scan.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the media library that is scanned.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the number of completed jobs of the media library scan job.
    /// </summary>
    public int CompletedJobs { get; set; }

    /// <summary>
    /// Gets or sets the total number of jobs of the media library scan job.
    /// </summary>
    public int TotalJobs { get; set; }

    /// <summary>
    /// Gets or sets the optional current job progress of the media library scan.
    /// </summary>
    public LibraryScanJobProgressModel? CurrentJobProgress { get; set; }

    /// <summary>
    /// Gets or sets the status of the media library scan.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the ratio between the total number and the completed number of jobs, as a percentage.
    /// </summary>
    public decimal OverallProgressPercentage { get; set; }
}
