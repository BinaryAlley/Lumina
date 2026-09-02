#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadBookViewRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadBookViewRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadBookViewRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book to read.</param>
    /// <returns>The created <see cref="ReadBookViewRequest"/>.</returns>
    public ReadBookViewRequest Create(
        Guid? bookId = null)
    {
        return new ReadBookViewRequest(bookId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="ReadBookViewRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadBookViewRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
