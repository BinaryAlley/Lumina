#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Progress;

/// <summary>
/// Interface for the tracker used for media library scans progress.
/// </summary>
public interface IMediaLibrariesScanProgressTracker
{
    /// <summary>
    /// Initializes the the progress tracking of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <param name="totalJobs">The total number of jobs that this media library scan contains.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    ErrorOr<Created> InitializeScanProgress(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, int totalJobs);

    /// <summary>
    /// Updates the progress tracking of the media library scan by incrementing the number of completed scan jobs.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    ErrorOr<Updated> UpdateScanProgress(MediaLibraryScanCompositeId mediaLibraryScanCompositeId);

    /// <summary>
    /// Updates the progress tracking of the media library scan by incrementing the number of completed items of a scan job.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <param name="progress">The object representing the progress of the media library scan job.</param>
    /// <returns>An <see cref="ErrorOr{TValue}"/> representing either a successful operation, or an error.</returns>
    ErrorOr<Updated> UpdateScanJobProgress(MediaLibraryScanCompositeId mediaLibraryScanCompositeId, MediaLibraryScanJobProgress progress);

    /// <summary>
    /// Gets the progress of a media library scan.
    /// </summary>
    /// <param name="mediaLibraryScanCompositeId">Model for tracking media library scans.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TValue}"/> containing either a <see cref="MediaLibraryScanProgress"/>, or an error message.
    /// </returns>
    ErrorOr<MediaLibraryScanProgress> GetScanProgress(MediaLibraryScanCompositeId mediaLibraryScanCompositeId);
}
