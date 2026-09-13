#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;
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
    /// <param name="libraryId">The Id of the library whose books are retrieved, taken from the route.</param>
    /// <returns>The converted query.</returns>
    public static GetBooksLiteQuery ToQuery(this GetBooksLiteRequest request, string? libraryId)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 200
            };

        return new GetBooksLiteQuery(
            libraryId,
            paginationData,
            request.SearchTerm,
            request.FilterAlphaKey,
            request.ShouldIgnoreThePrefixForAlphaPicker,
            request.SortBy,
            request.SortOrder
        );
    }
}
