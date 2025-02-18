#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a media library scan job progress response.
/// </summary>
/// <param name="CompletedItems">The number of processed scan job items.</param>
/// <param name="TotalItems">The total number of scan job items.</param>
/// <param name="CurrentOperation">The current processing operation in the scan job.</param>
/// <param name="ProgressPercentage">The ratio between the number of processed items and the total number of items to process, as percentage.</param>
[DebuggerDisplay("CompletedItems: {CompletedItems} TotalItems: {TotalItems}")]
public record MediaLibraryScanJobProgressResponse(
    int CompletedItems,
    int TotalItems,
    string CurrentOperation,
    decimal ProgressPercentage
);
