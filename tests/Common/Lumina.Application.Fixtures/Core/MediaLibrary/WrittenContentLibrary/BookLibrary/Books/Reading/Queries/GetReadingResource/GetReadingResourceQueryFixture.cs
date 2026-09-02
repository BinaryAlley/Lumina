#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingResource;

/// <summary>
/// Fixture class for the <see cref="GetReadingResourceQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get a resource of a book.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose resource is retrieved.</param>
    /// <param name="resourceKey">Optional. The opaque resource key of the resource.</param>
    /// <returns>The created <see cref="GetReadingResourceQuery"/>.</returns>
    public GetReadingResourceQuery Create(
        Guid? bookId = null,
        string? resourceKey = null)
    {
        return new GetReadingResourceQuery(bookId ?? Guid.NewGuid(), resourceKey ?? $"resource-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingResourceQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingResourceQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
