#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingSection;

/// <summary>
/// Fixture class for the <see cref="GetReadingSectionQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the content of a reading section of a book.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading section is retrieved.</param>
    /// <param name="locationRef">Optional. The opaque location reference of the reading section.</param>
    /// <returns>The created <see cref="GetReadingSectionQuery"/>.</returns>
    public GetReadingSectionQuery Create(
        Guid? bookId = null,
        string? locationRef = null)
    {
        return new GetReadingSectionQuery(bookId ?? Guid.NewGuid(), locationRef ?? $"section-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingSectionQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingSectionQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
