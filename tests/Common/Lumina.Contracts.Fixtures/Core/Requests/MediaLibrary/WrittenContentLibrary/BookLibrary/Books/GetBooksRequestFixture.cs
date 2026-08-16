#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Fixture class for the <see cref="GetBooksRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetBooksRequest"/> with default or random values.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose books are retrieved.</param>
    /// <param name="currentPage">Optional. The page of results to retrieve.</param>
    /// <param name="perPage">Optional. The maximum number of books to retrieve per page.</param>
    /// <param name="searchTerm">Optional. The search term used to filter results.</param>
    /// <param name="sortBy">Optional. The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional. The direction in which to sort the results.</param>
    /// <returns>The created <see cref="GetBooksRequest"/>.</returns>
    public GetBooksRequest Create(
        Guid? libraryId = null,
        int? currentPage = null,
        int? perPage = null,
        string? searchTerm = null,
        string? sortBy = null,
        SortOrder? sortOrder = null)
    {
        return new GetBooksRequest(
            LibraryId: libraryId ?? _faker.Random.Guid(),
            CurrentPage: currentPage ?? _faker.Random.Number(1, 100),
            PerPage: perPage ?? _faker.Random.Number(1, 200),
            SearchTerm: searchTerm ?? _faker.Lorem.Word(),
            SortBy: sortBy ?? _faker.Lorem.Word(),
            SortOrder: sortOrder ?? _faker.PickRandom<SortOrder>()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetBooksRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBooksRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
