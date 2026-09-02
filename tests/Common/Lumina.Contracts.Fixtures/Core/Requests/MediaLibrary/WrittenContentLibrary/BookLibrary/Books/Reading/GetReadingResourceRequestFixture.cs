#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="GetReadingResourceRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetReadingResourceRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose resource is retrieved.</param>
    /// <param name="resourceKey">Optional. The opaque resource key of the resource.</param>
    /// <returns>The created <see cref="GetReadingResourceRequest"/>.</returns>
    public GetReadingResourceRequest Create(
        Guid? bookId = null,
        string? resourceKey = null)
    {
        return new GetReadingResourceRequest(bookId ?? Guid.NewGuid(), resourceKey ?? $"resource-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingResourceRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingResourceRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
