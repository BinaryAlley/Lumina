#region ========================================================================= USING =====================================================================================
using ErrorOr;
#endregion

namespace Lumina.Domain.Common.Errors;

/// <summary>
/// Media Library Scan error types.
/// </summary>
public static partial class Errors
{
    public static class LibraryScanning
    {
        public static Error LibraryScanIdCannotBeEmpty => Error.Validation(description: nameof(LibraryScanIdCannotBeEmpty));
        public static Error CanOnlyStartPendingScans => Error.Forbidden(description: nameof(CanOnlyStartPendingScans));
        public static Error CanOnlyCompleteRunningScans => Error.Forbidden(description: nameof(CanOnlyCompleteRunningScans));
        public static Error CanOnlyCancelRunningScans => Error.Forbidden(description: nameof(CanOnlyCancelRunningScans));
        public static Error CanOnlyFailRunningScans => Error.Forbidden(description: nameof(CanOnlyFailRunningScans));
        public static Error ScanIdCannotBeEmpty => Error.Validation(description: nameof(ScanIdCannotBeEmpty));
        public static Error LibraryScanAlreadyExists => Error.Conflict(description: nameof(LibraryScanAlreadyExists));
        public static Error LibraryAlreadyBeingScanned => Error.Forbidden(description: nameof(LibraryAlreadyBeingScanned));
        public static Error LibraryScanNotFound => Error.NotFound(description: nameof(LibraryScanNotFound));
        public static Error TotalScanJobItemsCountMustBePositive => Error.Validation(description: nameof(TotalScanJobItemsCountMustBePositive));
        public static Error TotalScanJobsCountMustBePositive => Error.Validation(description: nameof(TotalScanJobsCountMustBePositive));
        public static Error CompletedScanJobItemsCountMustBePositive => Error.Validation(description: nameof(CompletedScanJobItemsCountMustBePositive));
        public static Error CompletedScanJobsCountMustBePositive => Error.Validation(description: nameof(CompletedScanJobsCountMustBePositive));
        public static Error CompletedScanJobItemsCountCantExceedTotalScanJobItemsCount => Error.Validation(description: nameof(CompletedScanJobItemsCountCantExceedTotalScanJobItemsCount));
        public static Error CompletedScanJobsCountCantExceedTotalScanJobsCount => Error.Validation(description: nameof(CompletedScanJobsCountCantExceedTotalScanJobsCount));
        public static Error ScanJobCurrentOperationCannotBeEmpty => Error.Validation(description: nameof(ScanJobCurrentOperationCannotBeEmpty));
    }
}
