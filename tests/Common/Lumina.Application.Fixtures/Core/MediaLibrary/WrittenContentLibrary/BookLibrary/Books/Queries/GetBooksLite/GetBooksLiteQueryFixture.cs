#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Fixture class for the <see cref="GetBooksLiteQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteQueryFixture
{
    private readonly Faker _faker = new();
    private readonly PaginationDataDtoFixture _paginationDataDtoFixture = new();

    /// <summary>
    /// Creates a random valid query to get the lightweight details of books.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose books are retrieved, taken from the route.</param>
    /// <param name="paginationData">Optional. The pagination data of the query.</param>
    /// <param name="searchTerm">Optional. The search term used to filter results.</param>
    /// <param name="filterAlphaKey">Optional. The alpha key used to filter the results by the first character of their title, for the alpha picker.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the leading "The " prefix of a title should be ignored when computing the alpha key, or not.</param>
    /// <param name="sortBy">Optional. The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional. The direction in which to sort the results.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePaginationData">Whether the pagination data should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created query to get the lightweight details of books.</returns>
    public GetBooksLiteQuery Create(
        string? libraryId = null,
        PaginationDataDto? paginationData = null,
        string? searchTerm = null,
        string? filterAlphaKey = null,
        bool shouldIgnoreThePrefixForAlphaPicker = false,
        string? sortBy = null,
        SortOrder? sortOrder = null,
        bool includeLibraryId = true,
        bool includePaginationData = true)
    {
        return new GetBooksLiteQuery(
            includeLibraryId ? (libraryId ?? _faker.Random.Guid().ToString()) : null,
            includePaginationData ? (paginationData ?? _paginationDataDtoFixture.Create()) : null,
            searchTerm,
            filterAlphaKey,
            shouldIgnoreThePrefixForAlphaPicker,
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
