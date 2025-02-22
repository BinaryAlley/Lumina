#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Collections.Concurrent;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;

/// <summary>
/// Tracker used for media library scans progress.
/// </summary>
internal class MediaLibrariesScanProgressTracker : IMediaLibrariesScanProgressTracker
{
    private readonly ConcurrentDictionary<MediaLibraryScanCompositeId, MediaLibraryScanProgress> _scanProgresses = new();

    /// <summary>
    /// Initializes the the progress tracking of a media library scan.
    /// </summary>
    /// <param name="libraryId">The object representing the unique identifier of the media library being scanned.</param>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <param name="totalJobs">The total number of jobs that this media library scan contains.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Created> InitializeScanProgress(LibraryId libraryId, MediaLibraryScanCompositeId mediaLibraryScanCompositeId, int totalJobs)
    {
        ErrorOr<MediaLibraryScanJobProgress> createLibraryScanJobProgressResult = MediaLibraryScanJobProgress.Create(0, 0, "Initializing");
        if (createLibraryScanJobProgressResult.IsError)
            return createLibraryScanJobProgressResult.Errors;

        ErrorOr<MediaLibraryScanProgress> createScanProgressResult = MediaLibraryScanProgress.Create(
            mediaLibraryScanCompositeId.ScanId,
            mediaLibraryScanCompositeId.UserId,
            libraryId,
            completedJobs: 0,
            totalJobs: totalJobs,
            LibraryScanJobStatus.Pending,
            Optional<MediaLibraryScanJobProgress>.Some(createLibraryScanJobProgressResult.Value)
        );
        if (createScanProgressResult.IsError)
            return createScanProgressResult.Errors;

        _scanProgresses[mediaLibraryScanCompositeId] = createScanProgressResult.Value;

        return Result.Created;
    }

    /// <summary>
    /// Updates the progress tracking of the media library scan by incrementing the number of completed scan jobs.
    /// </summary>
    /// <param name="libraryId">The object representing the unique identifier of the media library being scanned.</param>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Updated> UpdateScanProgress(LibraryId libraryId, MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {
        if (_scanProgresses.TryGetValue(mediaLibraryScanCompositeId, out MediaLibraryScanProgress? progress))
        {
            ErrorOr<MediaLibraryScanProgress> createScanProgressResult = MediaLibraryScanProgress.Create(
                mediaLibraryScanCompositeId.ScanId,
                mediaLibraryScanCompositeId.UserId,
                libraryId,
                progress.CompletedJobs + 1,
                progress.TotalJobs,
                progress.CompletedJobs + 1 == progress.TotalJobs ? LibraryScanJobStatus.Completed : LibraryScanJobStatus.Running,
                progress.CurrentJobProgress
            );
            if (createScanProgressResult.IsError)
                return createScanProgressResult.Errors;

            _scanProgresses[mediaLibraryScanCompositeId] = createScanProgressResult.Value;
        }
        return Result.Updated;
    }

    /// <summary>
    /// Updates the progress tracking of the media library scan by incrementing the number of completed items of a scan job.
    /// </summary>
    /// <param name="libraryId">The object representing the unique identifier of the media library being scanned.</param>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <param name="progress">The object representing the progress of the media library scan job.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    public ErrorOr<Updated> UpdateScanJobProgress(LibraryId libraryId, MediaLibraryScanCompositeId mediaLibraryScanCompositeId, MediaLibraryScanJobProgress progress)
    {
        if (_scanProgresses.TryGetValue(mediaLibraryScanCompositeId, out MediaLibraryScanProgress? scanProgress))
        {
            ErrorOr<MediaLibraryScanProgress> createScanProgressResult = MediaLibraryScanProgress.Create(
                mediaLibraryScanCompositeId.ScanId,
                mediaLibraryScanCompositeId.UserId,
                libraryId,
                scanProgress.CompletedJobs,
                scanProgress.TotalJobs,
                scanProgress.Status,
                Optional<MediaLibraryScanJobProgress>.Some(progress)
            );
            if (createScanProgressResult.IsError)
                return createScanProgressResult.Errors;

            _scanProgresses[mediaLibraryScanCompositeId] = createScanProgressResult.Value;
            return Result.Updated;
        }
        return Errors.LibraryScanning.LibraryScanNotFound;
    }

    /// <summary>
    /// Gets the progress of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="MediaLibraryScanProgress"/>, or an error message.
    /// </returns>
    public ErrorOr<MediaLibraryScanProgress> GetScanProgress(MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {        
        if (_scanProgresses.TryGetValue(mediaLibraryScanCompositeId, out MediaLibraryScanProgress? progress))
            return progress;
        return Errors.LibraryScanning.LibraryScanNotFound;
    }

    /// <summary>
    /// Removes the progress of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="MediaLibraryScanProgress"/>, or an error message.
    /// </returns>
    public ErrorOr<MediaLibraryScanProgress> RemoveScanProgress(MediaLibraryScanCompositeId mediaLibraryScanCompositeId)
    {
        if (_scanProgresses.TryRemove(mediaLibraryScanCompositeId, out MediaLibraryScanProgress? progress))
            return progress;
        return Errors.LibraryScanning.LibraryScanNotFound;
    }
}
