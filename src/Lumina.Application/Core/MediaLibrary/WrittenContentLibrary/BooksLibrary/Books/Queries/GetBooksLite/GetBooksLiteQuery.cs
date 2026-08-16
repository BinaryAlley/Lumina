#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Filtering;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
#endregion

namespace Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooksLite;

/// <summary>
/// Query for getting the lightweight details of all the books of a media library.
/// </summary>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
/// <param name="Filter">The object containing the criteria used to filter the results.</param>
/// <param name="SortBy">The name of the field by which to sort the results.</param>
/// <param name="SortOrder">The direction in which to sort the results.</param>
public record GetBooksLiteQuery(
    PaginationDataDto? PaginationData,
    LibraryFilterDto Filter,
    string? SortBy,
    SortOrder? SortOrder
) : IQuery;
