#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="GetBookReadingSectionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookReadingSectionRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetBookReadingSectionRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading section is retrieved.</param>
    /// <param name="locationRef">Optional. The opaque location reference of the reading section.</param>
    /// <returns>The created <see cref="GetBookReadingSectionRequest"/>.</returns>
    public GetBookReadingSectionRequest Create(
        Guid? bookId = null,
        string? locationRef = null)
    {
        return new GetBookReadingSectionRequest(bookId ?? Guid.NewGuid(), locationRef ?? $"section-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Creates a list of <see cref="GetBookReadingSectionRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBookReadingSectionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
