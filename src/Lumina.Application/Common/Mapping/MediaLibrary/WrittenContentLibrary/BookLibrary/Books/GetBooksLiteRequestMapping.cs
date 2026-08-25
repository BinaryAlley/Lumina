#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetBooksLiteRequest"/>.
/// </summary>
public static class GetBooksLiteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetBooksLiteQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetBooksLiteQuery ToQuery(this GetBooksLiteRequest request)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
        {
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };
        }
        LibraryFilterDto libraryFilter = new()
        {
            LibraryId = request.LibraryId,
            SearchTerm = request.SearchTerm,
            FilterAlphaKey = request.FilterAlphaKey,
            ShouldIgnoreThePrefixForAlphaPicker = request.ShouldIgnoreThePrefixForAlphaPicker
        };

        return new GetBooksLiteQuery(paginationData, libraryFilter, request.SortBy, request.SortOrder);
    }
}
