#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Nemera.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;

/// <summary>
/// Fixture class for the <see cref="GetDeletedBooksQuery"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDeletedBooksQueryFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetDeletedBooksQuery"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose deleted books are retrieved, taken from the route.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetDeletedBooksQuery"/>.</returns>
    public GetDeletedBooksQuery Create(
        string? libraryId = null,
        bool includeLibraryId = true)
    {
        return new GetDeletedBooksQuery(
            includeLibraryId ? (libraryId ?? Guid.NewGuid().ToString()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetDeletedBooksQuery"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetDeletedBooksQuery"/> instances.</returns>
    public List<GetDeletedBooksQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
