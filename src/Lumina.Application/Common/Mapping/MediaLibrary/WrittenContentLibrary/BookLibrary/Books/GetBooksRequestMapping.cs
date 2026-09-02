#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="GetBooksRequest"/>.
/// </summary>
public static class GetBooksRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetBooksQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetBooksQuery ToQuery(this GetBooksRequest request)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };

        LibraryFilterDto libraryFilter = new()
        {
            LibraryId = request.LibraryId,
            SearchTerm = request.SearchTerm
        };

        return new GetBooksQuery(paginationData, libraryFilter, request.SortBy, request.SortOrder);
    }
}
