#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Common;
using Lumina.Presentation.Web.Common.Requests.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Libraries;

/// <summary>
/// Fixture class for generating <see cref="GetBooksLiteRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBooksLiteRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetBooksLiteRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="libraryId">Optional identifier of the media library.</param>
    /// <param name="currentPage">Optional page of results to retrieve.</param>
    /// <param name="perPage">Optional number of books to retrieve per page.</param>
    /// <param name="searchTerm">Optional search term used to filter results.</param>
    /// <param name="filterAlphaKey">Optional alpha key used to filter the results.</param>
    /// <param name="shouldIgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix is ignored by the alpha picker.</param>
    /// <param name="sortBy">Optional name of the field by which to sort the results.</param>
    /// <param name="sortOrder">Optional direction in which to sort the results.</param>
    /// <returns>A configured <see cref="GetBooksLiteRequest"/> instance.</returns>
    public GetBooksLiteRequest Create(
        Guid? libraryId = null,
        int? currentPage = null,
        int? perPage = null,
        string? searchTerm = null,
        string? filterAlphaKey = null,
        bool shouldIgnoreThePrefixForAlphaPicker = false,
        string? sortBy = null,
        SortOrder? sortOrder = null)
    {
        return new GetBooksLiteRequest
        {
            LibraryId = libraryId ?? Guid.NewGuid(),
            CurrentPage = currentPage,
            PerPage = perPage,
            SearchTerm = searchTerm,
            FilterAlphaKey = filterAlphaKey,
            ShouldIgnoreThePrefixForAlphaPicker = shouldIgnoreThePrefixForAlphaPicker,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
    }

    /// <summary>
    /// Creates multiple <see cref="GetBooksLiteRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetBooksLiteRequest"/> instances.</returns>
    public List<GetBooksLiteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
