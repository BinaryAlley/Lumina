#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Value Object representing a progress snapshot of a media library scan job.
/// </summary>
[DebuggerDisplay("CompletedItems: {CompletedItems}; TotalItems: {TotalItems}; CurrentOperation: {CurrentOperation}")]
public sealed class MediaLibraryScanJobProgress : ValueObject
{
    /// <summary>
    /// Gets the number of completed items of the media library scan job.
    /// </summary>
    public int CompletedItems { get; }

    /// <summary>
    /// Gets the total number of items of the media library scan job.
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// Gets the current operation being performed by the media library scan job.
    /// </summary>
    public string CurrentOperation { get; } = string.Empty;

    /// <summary>
    /// Gets the ratio between the total number and the completed number of items, as a percentage.
    /// </summary>
    public decimal ProgressPercentage => TotalItems > 0 ? CompletedItems * 100.0m / TotalItems : 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanJobProgress"/> class.
    /// </summary>
    /// <param name="completedItems">The number of completed items of the media library scan job.</param>
    /// <param name="totalItems">The total number of items of the media library scan job.</param>
    /// <param name="currentOperation">The current operation being performed by the media library scan job.</param>
    private MediaLibraryScanJobProgress(int completedItems, int totalItems, string currentOperation)
    {
        CompletedItems = completedItems;
        TotalItems = totalItems;
        CurrentOperation = currentOperation;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaLibraryScanJobProgress"/> class.
    /// </summary>
    /// <param name="completedItems">The number of completed items of the media library scan job.</param>
    /// <param name="totalItems">The total number of items of the media library scan job.</param>
    /// <param name="currentOperation">The current operation being performed by the media library scan job.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="MediaLibraryScanJobProgress"/>, or an error message.
    /// </returns>
    public static ErrorOr<MediaLibraryScanJobProgress> Create(int completedItems, int totalItems, string currentOperation)
    {
        if (totalItems < 0)
            return Errors.LibraryScanning.TotalScanJobItemsCountMustBePositive;
        if (completedItems < 0)
            return Errors.LibraryScanning.CompletedScanJobItemsCountMustBePositive;
        if (completedItems > totalItems)
            return Errors.LibraryScanning.CompletedScanJobItemsCountCantExceedTotalScanJobItemsCount;
        if (string.IsNullOrWhiteSpace(currentOperation))
            return Errors.LibraryScanning.ScanJobCurrentOperationCannotBeEmpty;
        return new MediaLibraryScanJobProgress(completedItems, totalItems, currentOperation);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return CompletedItems;
        yield return TotalItems;
        yield return CurrentOperation;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        return $"CompletedItems: {CompletedItems}; TotalItems: {TotalItems}; CurrentOperation: {CurrentOperation}";
    }
}
