#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaLibraryScanCompositeId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanCompositeIdFixture
{
    private readonly ScanIdFixture _scanIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaLibraryScanCompositeId"/>.
    /// </summary>
    /// <param name="scanId">Optional. The scan Id of the composite scan identifier.</param>
    /// <param name="userId">Optional. The user Id of the composite scan identifier.</param>
    /// <returns>The created <see cref="MediaLibraryScanCompositeId"/>.</returns>
    public MediaLibraryScanCompositeId Create(ScanId? scanId = null, UserId? userId = null)
    {
        return MediaLibraryScanCompositeId.Create(
            scanId ?? _scanIdFixture.Create(),
            userId ?? _userIdFixture.Create());
    }

    /// <summary>
    /// Creates a list of <see cref="MediaLibraryScanCompositeId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaLibraryScanCompositeId"/> instances.</returns>
    public List<MediaLibraryScanCompositeId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
