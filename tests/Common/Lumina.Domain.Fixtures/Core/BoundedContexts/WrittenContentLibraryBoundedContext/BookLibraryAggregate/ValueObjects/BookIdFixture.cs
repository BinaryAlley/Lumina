#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="BookId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="BookId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="BookId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="BookId"/>.</returns>
    public BookId Create(Guid? value = null)
    {
        return value is null ? BookId.CreateUnique() : BookId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="BookId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookId"/> instances.</returns>
    public List<BookId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
