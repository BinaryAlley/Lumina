#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Jobs;

/// <summary>
/// Fixture class for the <see cref="IMediaLibraryScanDiscoveryJob"/> interface.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanDiscoveryJobFixture
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly LibraryIdFixture _libraryIdFixture = new();

    /// <summary>
    /// Creates a controllable <see cref="FakeMediaLibraryScanDiscoveryJob"/>.
    /// </summary>
    /// <param name="scanId">Optional. The Id of the media library scan that the job is part of.</param>
    /// <param name="userId">Optional. The Id of the user that initiated the media library scan.</param>
    /// <param name="libraryId">Optional. The Id of the media library that is scanned.</param>
    /// <param name="children">Optional. The child jobs of the created job.</param>
    /// <param name="onExecute">Optional. The callback invoked while the job executes.</param>
    /// <returns>The created <see cref="FakeMediaLibraryScanDiscoveryJob"/>.</returns>
    public FakeMediaLibraryScanDiscoveryJob Create(
        ScanId? scanId = null,
        UserId? userId = null,
        LibraryId? libraryId = null,
        List<IMediaLibraryScanJob>? children = null,
        Func<CancellationToken, Task>? onExecute = null)
    {
        FakeMediaLibraryScanDiscoveryJob job = new()
        {
            ScanId = scanId ?? _scanIdFixture.Create(),
            UserId = userId ?? _userIdFixture.Create(),
            LibraryId = libraryId ?? _libraryIdFixture.Create(),
            OnExecute = onExecute
        };

        foreach (IMediaLibraryScanJob child in children ?? [])
            job.AddChild(child);

        return job;
    }
}
