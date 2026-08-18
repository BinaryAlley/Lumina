#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="ScanId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ScanId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the scan Id.</param>
    /// <returns>The created <see cref="ScanId"/>.</returns>
    public ScanId Create(Guid? value = null)
    {
        return ScanId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="ScanId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ScanId"/> instances.</returns>
    public List<ScanId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
