#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
#endregion

namespace Nemera.Domain.Common.Errors;

/// <summary>
/// Media Library Scan error types.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Groups the errors that report media library scan rule violations and missing scans.
    /// </summary>
    public static class LibraryScanning
    {
        /// <summary>
        /// Gets the error that reports that the library scan identifier cannot be empty.
        /// </summary>
        public static Error LibraryScanIdCannotBeEmpty => Error.Validation(description: nameof(LibraryScanIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that only a pending scan can be started.
        /// </summary>
        public static Error CanOnlyStartPendingScans => Error.Forbidden(description: nameof(CanOnlyStartPendingScans));

        /// <summary>
        /// Gets the error that reports that only a running scan can be completed.
        /// </summary>
        public static Error CanOnlyCompleteRunningScans => Error.Forbidden(description: nameof(CanOnlyCompleteRunningScans));

        /// <summary>
        /// Gets the error that reports that only a pending or running scan can be cancelled.
        /// </summary>
        public static Error CanOnlyCancelPendingOrRunningScans => Error.Forbidden(description: nameof(CanOnlyCancelPendingOrRunningScans));

        /// <summary>
        /// Gets the error that reports that only a pending or running scan can be marked as failed.
        /// </summary>
        public static Error CanOnlyFailPendingOrRunningScans => Error.Forbidden(description: nameof(CanOnlyFailPendingOrRunningScans));

        /// <summary>
        /// Gets the error that reports that the scan identifier cannot be empty.
        /// </summary>
        public static Error ScanIdCannotBeEmpty => Error.Validation(description: nameof(ScanIdCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that a media library scan with the same identifier already exists.
        /// </summary>
        public static Error LibraryScanAlreadyExists => Error.Conflict(description: nameof(LibraryScanAlreadyExists));

        /// <summary>
        /// Gets the error that reports that the library is already being scanned.
        /// </summary>
        public static Error LibraryAlreadyBeingScanned => Error.Forbidden(description: nameof(LibraryAlreadyBeingScanned));

        /// <summary>
        /// Gets the error that reports that the requested library scan was not found.
        /// </summary>
        public static Error LibraryScanNotFound => Error.NotFound(description: nameof(LibraryScanNotFound));

        /// <summary>
        /// Gets the error that reports that the total scan job items count cannot be negative.
        /// </summary>
        public static Error TotalScanJobItemsCountMustBePositive => Error.Validation(description: nameof(TotalScanJobItemsCountMustBePositive));

        /// <summary>
        /// Gets the error that reports that the total scan jobs count cannot be negative, and cannot be zero unless the scan is pending.
        /// </summary>
        public static Error TotalScanJobsCountMustBePositive => Error.Validation(description: nameof(TotalScanJobsCountMustBePositive));

        /// <summary>
        /// Gets the error that reports that the completed scan job items count cannot be negative.
        /// </summary>
        public static Error CompletedScanJobItemsCountMustBePositive => Error.Validation(description: nameof(CompletedScanJobItemsCountMustBePositive));

        /// <summary>
        /// Gets the error that reports that the completed scan jobs count cannot be negative.
        /// </summary>
        public static Error CompletedScanJobsCountMustBePositive => Error.Validation(description: nameof(CompletedScanJobsCountMustBePositive));

        /// <summary>
        /// Gets the error that reports that the completed scan job items count cannot exceed the total scan job items count.
        /// </summary>
        public static Error CompletedScanJobItemsCountCantExceedTotalScanJobItemsCount => Error.Validation(description: nameof(CompletedScanJobItemsCountCantExceedTotalScanJobItemsCount));

        /// <summary>
        /// Gets the error that reports that the completed scan jobs count cannot exceed the total scan jobs count.
        /// </summary>
        public static Error CompletedScanJobsCountCantExceedTotalScanJobsCount => Error.Validation(description: nameof(CompletedScanJobsCountCantExceedTotalScanJobsCount));

        /// <summary>
        /// Gets the error that reports that the current operation of a scan job cannot be empty.
        /// </summary>
        public static Error ScanJobCurrentOperationCannotBeEmpty => Error.Validation(description: nameof(ScanJobCurrentOperationCannotBeEmpty));

        /// <summary>
        /// Gets the error that reports that no scanner is implemented for the requested library type.
        /// </summary>
        public static Error NoScannerForLibraryType => Error.NotImplemented(description: nameof(NoScannerForLibraryType));
    }
}
