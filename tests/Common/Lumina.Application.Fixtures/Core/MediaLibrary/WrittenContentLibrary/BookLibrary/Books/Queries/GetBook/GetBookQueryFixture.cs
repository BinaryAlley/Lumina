#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;

/// <summary>
/// Fixture class for the <see cref="GetBookQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetBookQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the library the book belongs to.</param>
    /// <param name="bookId">Optional. The Id of the book to get.</param>
    /// <returns>The created <see cref="GetBookQuery"/>.</returns>
    public GetBookQuery Create(
        string? libraryId = null,
        string? bookId = null)
    {
        return new GetBookQuery(libraryId ?? Guid.NewGuid().ToString(), bookId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a list of <see cref="GetBookQuery"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetBookQuery"/> instances.</returns>
    public List<GetBookQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
