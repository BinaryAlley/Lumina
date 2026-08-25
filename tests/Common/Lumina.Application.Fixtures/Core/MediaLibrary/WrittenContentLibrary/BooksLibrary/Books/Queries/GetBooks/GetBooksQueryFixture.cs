#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;

/// <summary>
/// Fixture class for the <see cref="GetBooksQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid query to get books.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose books are retrieved.</param>
    /// <param name="paginationData">Optional. The pagination data of the query.</param>
    /// <param name="searchTerm">Optional. The search term used to filter results.</param>
    /// <param name="sortBy">Optional. The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional. The direction in which to sort the results.</param>
    /// <param name="includePaginationData">Whether the query should include pagination data or not.</param>
    /// <returns>The created query to get books.</returns>
    public GetBooksQuery Create(
        Guid? libraryId = null,
        PaginationDataDto? paginationData = null,
        string? searchTerm = null,
        string? sortBy = null,
        SortOrder? sortOrder = null,
        bool includePaginationData = true)
    {
        return new GetBooksQuery(
            includePaginationData ? paginationData ?? new PaginationDataDto
            {
                CurrentPage = _faker.Random.Number(1, 100),
                PerPage = _faker.Random.Number(1, 200)
            } : null,
            new LibraryFilterDto
            {
                LibraryId = libraryId ?? _faker.Random.Guid(),
                SearchTerm = searchTerm
            },
            sortBy,
            sortOrder ?? _faker.PickRandom<SortOrder>()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetBooksQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBooksQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
