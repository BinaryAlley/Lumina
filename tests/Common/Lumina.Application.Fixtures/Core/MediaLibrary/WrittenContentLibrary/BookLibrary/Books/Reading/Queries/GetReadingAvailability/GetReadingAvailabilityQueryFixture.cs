#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingAvailability;

/// <summary>
/// Fixture class for the <see cref="GetReadingAvailabilityQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityQueryFixture
{
    /// <summary>
    /// Creates a random valid query to check the reading availability of a book.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the library the book belongs to.</param>
    /// <param name="bookId">Optional. The Id of the book whose reading availability is checked.</param>
    /// <returns>The created <see cref="GetReadingAvailabilityQuery"/>.</returns>
    public GetReadingAvailabilityQuery Create(
        string? libraryId = null,
        string? bookId = null)
    {
        return new GetReadingAvailabilityQuery(libraryId ?? Guid.NewGuid().ToString(), bookId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingAvailabilityQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingAvailabilityQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
