#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="GetReadingAvailabilityRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingAvailabilityRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetReadingAvailabilityRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading availability is checked.</param>
    /// <returns>The created <see cref="GetReadingAvailabilityRequest"/>.</returns>
    public GetReadingAvailabilityRequest Create(
        Guid? bookId = null)
    {
        return new GetReadingAvailabilityRequest(bookId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingAvailabilityRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingAvailabilityRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
