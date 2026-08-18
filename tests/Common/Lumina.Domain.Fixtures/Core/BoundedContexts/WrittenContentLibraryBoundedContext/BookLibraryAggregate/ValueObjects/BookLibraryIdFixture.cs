#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="BookLibraryId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookLibraryIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="BookLibraryId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="BookLibraryId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="BookLibraryId"/>.</returns>
    public BookLibraryId Create(Guid? value = null)
    {
        return value is null ? BookLibraryId.CreateUnique().Value : BookLibraryId.Create(value.Value).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="BookLibraryId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookLibraryId"/> instances.</returns>
    public List<BookLibraryId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
