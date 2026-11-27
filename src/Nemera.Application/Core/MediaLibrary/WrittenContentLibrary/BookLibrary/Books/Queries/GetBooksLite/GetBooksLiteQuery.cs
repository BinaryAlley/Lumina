#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Query for getting the lightweight details of all the books of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose books are retrieved, taken from the route.</param>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
/// <param name="SearchTerm">The search term used to filter results.</param>
/// <param name="FilterAlphaKey">The alpha key used to filter results by the first character of their title.</param>
/// <param name="ShouldIgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix of a title should be ignored when computing the alpha key, or not.</param>
/// <param name="SortBy">The name of the field by which to sort the results.</param>
/// <param name="SortOrder">The direction in which to sort the results.</param>
public record GetBooksLiteQuery(
    string? LibraryId,
    PaginationDataDto? PaginationData,
    string? SearchTerm,
    string? FilterAlphaKey,
    bool ShouldIgnoreThePrefixForAlphaPicker,
    string? SortBy,
    SortOrder? SortOrder
) : IQuery;
