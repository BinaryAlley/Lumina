#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaLibraryScanProgress"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanProgressFixture
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaLibraryScanProgress"/>.
    /// </summary>
    /// <param name="scanId">Optional. The scan Id of the media library scan.</param>
    /// <param name="userId">Optional. The user Id of the user initiating the media library scan.</param>
    /// <param name="libraryId">Optional. The library Id of the scanned media library.</param>
    /// <param name="completedJobs">Optional. The number of completed jobs of the media library scan.</param>
    /// <param name="totalJobs">Optional. The total number of jobs of the media library scan.</param>
    /// <param name="status">Optional. The status of the media library scan.</param>
    /// <param name="currentJobProgress">Optional. The current job progress of the media library scan. When <see langword="null"/>, no current job progress is set.</param>
    /// <returns>The created <see cref="MediaLibraryScanProgress"/>.</returns>
    public MediaLibraryScanProgress Create(
        ScanId? scanId = null,
        UserId? userId = null,
        LibraryId? libraryId = null,
        int? completedJobs = null,
        int? totalJobs = null,
        LibraryScanJobStatus? status = null,
        Optional<MediaLibraryScanJobProgress>? currentJobProgress = null)
    {
        Result<MediaLibraryScanProgress> progress = MediaLibraryScanProgress.Create(
            scanId ?? _scanIdFixture.Create(),
            userId ?? _userIdFixture.Create(),
            libraryId ?? _libraryIdFixture.Create(),
            completedJobs ?? 1,
            totalJobs ?? 2,
            status ?? LibraryScanJobStatus.Pending,
            currentJobProgress ?? Optional<MediaLibraryScanJobProgress>.None());

        return progress.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="MediaLibraryScanProgress"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaLibraryScanProgress"/> instances.</returns>
    public List<MediaLibraryScanProgress> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
