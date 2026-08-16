#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Fixture class for the <see cref="GetBooksLiteQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid query to get the lightweight details of books.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose books are retrieved.</param>
    /// <param name="paginationData">Optional. The pagination data of the query.</param>
    /// <param name="searchTerm">Optional. The search term used to filter results.</param>
    /// <param name="filterAlphaKey">Optional. The alpha key used to filter the results by the first character of their title, for the alpha picker.</param>
    /// <param name="ignoreThePrefixForAlphaPicker">Optional. Whether the leading "The " prefix of a title should be ignored when computing the alpha key, or not.</param>
    /// <param name="sortBy">Optional. The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional. The direction in which to sort the results.</param>
    /// <returns>The created query to get the lightweight details of books.</returns>
    public GetBooksLiteQuery Create(
        Guid? libraryId = null,
        PaginationDataDto? paginationData = null,
        string? searchTerm = null,
        string? filterAlphaKey = null,
        bool ignoreThePrefixForAlphaPicker = false,
        string? sortBy = null,
        SortOrder? sortOrder = null)
    {
        return new GetBooksLiteQuery(
            paginationData ?? new PaginationDataDto
            {
                CurrentPage = _faker.Random.Number(1, 100),
                PerPage = _faker.Random.Number(1, 200)
            },
            new LibraryFilterDto
            {
                LibraryId = libraryId ?? _faker.Random.Guid(),
                SearchTerm = searchTerm,
                FilterAlphaKey = filterAlphaKey,
                IgnoreThePrefixForAlphaPicker = ignoreThePrefixForAlphaPicker
            },
            sortBy,
            sortOrder ?? _faker.PickRandom<SortOrder>()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetBooksLiteQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBooksLiteQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
