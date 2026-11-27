#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="BookSeriesId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookSeriesIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="BookSeriesId"/>.
    /// </summary>
    /// <param name="value">Optional. The value used to create the <see cref="BookSeriesId"/>. If not provided, a random value is generated.</param>
    /// <returns>The created <see cref="BookSeriesId"/>.</returns>
    public BookSeriesId Create(Guid? value = null)
    {
        return value is null ? BookSeriesId.CreateUnique() : BookSeriesId.Create(value.Value);
    }

    /// <summary>
    /// Creates multiple <see cref="BookSeriesId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookSeriesId"/> instances.</returns>
    public List<BookSeriesId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
