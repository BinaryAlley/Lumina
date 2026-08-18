#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate;

/// <summary>
/// Fixture class for the <see cref="LibraryScan"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryScan"/> domain aggregate.
    /// </summary>
    /// <param name="id">Optional. The scan Id.</param>
    /// <param name="libraryId">Optional. The Id of the media library that is scanned.</param>
    /// <param name="userId">Optional. The Id of the user that initiated the scan.</param>
    /// <param name="status">Optional. The status of the media library scan.</param>
    /// <param name="pastScans">Optional. The list of past scans of the scanned media library.</param>
    /// <returns>The created <see cref="LibraryScan"/>.</returns>
    public LibraryScan Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? userId = null,
        LibraryScanJobStatus? status = null,
        List<LibraryScan>? pastScans = null)
    {
        Result<LibraryScan> scan = LibraryScan.Create(
            ScanId.Create(id ?? Guid.NewGuid()),
            LibraryId.Create(libraryId ?? Guid.NewGuid()),
            UserId.Create(userId ?? Guid.NewGuid()),
            status ?? LibraryScanJobStatus.Pending,
            pastScans ?? []);

        return scan.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryScan"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryScan"/> instances.</returns>
    public List<LibraryScan> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
