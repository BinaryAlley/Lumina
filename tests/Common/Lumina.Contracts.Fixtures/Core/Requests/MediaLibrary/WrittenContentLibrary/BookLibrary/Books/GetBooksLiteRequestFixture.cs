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
/// Fixture class for the <see cref="GetBooksLiteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetBooksLiteRequest"/> with default or random values.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose books are retrieved.</param>
    /// <param name="currentPage">Optional. The page of results to retrieve.</param>
    /// <param name="perPage">Optional. The maximum number of books to retrieve per page.</param>
    /// <param name="searchTerm">Optional. The search term used to filter results.</param>
    /// <param name="filterAlphaKey">Optional. The alpha key used to filter the results by the first character of their title, for the alpha picker.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Optional. Whether the leading "The " prefix of a title should be ignored when computing the alpha key, or not.</param>
    /// <param name="sortBy">Optional. The name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional. The direction in which to sort the results.</param>
    /// <param name="includeCurrentPage">Whether the current page should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePerPage">Whether the per page count should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetBooksLiteRequest"/>.</returns>
    public GetBooksLiteRequest Create(
        Guid? libraryId = null,
        int? currentPage = null,
        int? perPage = null,
        string? searchTerm = null,
        string? filterAlphaKey = null,
        bool shouldIgnoreThePrefixForAlphaPicker = false,
        string? sortBy = null,
        SortOrder? sortOrder = null,
        bool includeCurrentPage = true,
        bool includePerPage = true)
    {
        return new GetBooksLiteRequest(
            LibraryId: libraryId ?? _faker.Random.Guid(),
            CurrentPage: includeCurrentPage ? (currentPage ?? _faker.Random.Number(1, 100)) : null,
            PerPage: includePerPage ? (perPage ?? _faker.Random.Number(1, 200)) : null,
            SearchTerm: searchTerm ?? _faker.Lorem.Word(),
            FilterAlphaKey: filterAlphaKey,
            ShouldIgnoreThePrefixForAlphaPicker: shouldIgnoreThePrefixForAlphaPicker,
            SortBy: sortBy ?? _faker.Lorem.Word(),
            SortOrder: sortOrder ?? _faker.PickRandom<SortOrder>()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetBooksLiteRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBooksLiteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
