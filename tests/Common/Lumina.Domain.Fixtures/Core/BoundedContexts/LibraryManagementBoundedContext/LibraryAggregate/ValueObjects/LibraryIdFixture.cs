#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="LibraryId"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the library Id.</param>
    /// <returns>The created <see cref="LibraryId"/>.</returns>
    public LibraryId Create(Guid? value = null)
    {
        return LibraryId.Create(value ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryId"/> instances.</returns>
    public List<LibraryId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
