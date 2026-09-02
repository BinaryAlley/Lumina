#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="GetBookReadingAvailabilityRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookReadingAvailabilityRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetBookReadingAvailabilityRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading availability is checked.</param>
    /// <returns>The created <see cref="GetBookReadingAvailabilityRequest"/>.</returns>
    public GetBookReadingAvailabilityRequest Create(
        Guid? bookId = null)
    {
        return new GetBookReadingAvailabilityRequest(bookId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetBookReadingAvailabilityRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBookReadingAvailabilityRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
