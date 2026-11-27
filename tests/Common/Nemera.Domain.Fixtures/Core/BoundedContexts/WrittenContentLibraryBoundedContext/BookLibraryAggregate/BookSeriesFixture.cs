#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.Entities;
using Lumina.Domain.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.WrittenContentLibraryBoundedContext.BookLibraryAggregate;

/// <summary>
/// Fixture class for the <see cref="BookSeries"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class BookSeriesFixture
{
    private readonly BookSeriesIdFixture _bookSeriesIdFixture = new();
    private readonly WrittenContentMetadataFixture _writtenContentMetadataFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="BookSeries"/>.
    /// </summary>
    /// <param name="id">Optional. The unique identifier of the book series.</param>
    /// <param name="isComplete">Whether the book series is complete.</param>
    /// <returns>The created <see cref="BookSeries"/>.</returns>
    public BookSeries Create(
        Guid? id = null,
        bool isComplete = false)
    {
        return BookSeries.Create(
            id is null ? BookSeriesId.CreateUnique() : _bookSeriesIdFixture.Create(id.Value),
            _writtenContentMetadataFixture.Create(),
            isComplete,
            []).Value;
    }

    /// <summary>
    /// Creates multiple <see cref="BookSeries"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BookSeries"/> instances.</returns>
    public List<BookSeries> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
