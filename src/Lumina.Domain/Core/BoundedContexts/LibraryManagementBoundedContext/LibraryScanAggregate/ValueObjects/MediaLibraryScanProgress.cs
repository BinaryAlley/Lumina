#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Value Object representing a progress snapshot of a media library scan.
/// </summary>
[DebuggerDisplay("CompletedJobs: {CompletedJobs}; TotalJobs: {TotalJobs}; CurrentJobProgress: {CurrentJobProgress}")]
public sealed class MediaLibraryScanProgress : ValueObject
{
    /// <summary>
    /// Gets the number of completed jobs of the media library scan job.
    /// </summary>
    public int CompletedJobs { get; }

    /// <summary>
    /// Gets the total number of jobs of the media library scan job.
    /// </summary>
    public int TotalJobs { get; }

    /// <summary>
    /// Gets the optional current job progress of the media library scan.
    /// </summary>
    public Optional<MediaLibraryScanJobProgress> CurrentJobProgress { get; } = Optional<MediaLibraryScanJobProgress>.None();

    /// <summary>
    /// Gets the status of the media library scan.
    /// </summary>
    public LibraryScanJobStatus Status { get; } = LibraryScanJobStatus.Pending;

    /// <summary>
    /// Gets the ratio between the total number and the completed number of jobs, as a percentage.
    /// </summary>
    public decimal OverallProgressPercentage => TotalJobs > 0 ? CompletedJobs * 100.0m / TotalJobs : 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanProgress"/> class.
    /// </summary>
    /// <param name="completedJobs">The number of completed jobs of the media library scan job.</param>
    /// <param name="totalJobs">The total number of jobs of the media library scan job.</param>
    /// <param name="status">The status of the media library scan.</param>
    /// <param name="currentJobProgress">The optional current job progress of the media library scan.</param>
    private MediaLibraryScanProgress(int completedJobs, int totalJobs, LibraryScanJobStatus status, Optional<MediaLibraryScanJobProgress> currentJobProgress)
    {
        CompletedJobs = completedJobs;
        TotalJobs = totalJobs;
        CurrentJobProgress = currentJobProgress;
        Status = status;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaLibraryScanProgress"/> class.
    /// </summary>
    /// <param name="completedJobs">The number of completed jobs of the media library scan job.</param>
    /// <param name="totalJobs">The total number of jobs of the media library scan job.</param>
    /// <param name="status">The status of the media library scan.</param>
    /// <param name="currentJobProgress">The optional current job progress of the media library scan.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a successfuly created <see cref="MediaLibraryScanProgress"/>, or an error message.
    /// </returns>
    public static ErrorOr<MediaLibraryScanProgress> Create(int completedJobs, int totalJobs, LibraryScanJobStatus status, Optional<MediaLibraryScanJobProgress> currentJobProgress)
    {
        if (totalJobs <= 0)
            return Errors.LibraryScanning.TotalScanJobsCountMustBePositive;
        if (completedJobs < 0)
            return Errors.LibraryScanning.CompletedScanJobsCountMustBePositive;
        if (completedJobs > totalJobs)
            return Errors.LibraryScanning.CompletedScanJobsCountCantExceedTotalScanJobsCount;

        return new MediaLibraryScanProgress(completedJobs, totalJobs, status, currentJobProgress);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return CompletedJobs;
        yield return TotalJobs;
        yield return Status;
        yield return CurrentJobProgress;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        return $"CompletedJobs: {CompletedJobs}; TotalJobs: {TotalJobs}; CurrentJobProgress: {CurrentJobProgress}";
    }
}
