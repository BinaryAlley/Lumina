#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;

/// <summary>
/// Extension methods for converting <see cref="BookLiteRow"/>.
/// </summary>
public static class BookLiteRowMapping
{
    /// <summary>
    /// Converts <paramref name="readModel"/> to <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="readModel">The read model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static BookLiteResponse ToResponse(this BookLiteRow readModel)
    {
        return new BookLiteResponse(
            readModel.Id,
            readModel.Title,
            readModel.ReleaseYear,
            readModel.CoverPath);
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a collection of <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The read models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<BookLiteResponse> ToResponses(this IEnumerable<BookLiteRow> readModels)
    {
        return [.. readModels.Select(readModel => readModel.ToResponse())];
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a paginated collection of <see cref="BookLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The paginated read models to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<BookLiteResponse> ToResponses(this PaginatedResultDto<BookLiteRow> readModels)
    {
        return new PaginatedResponse<BookLiteResponse>
        {
            Data = readModels.Data.ToResponses(),
            CurrentPage = readModels.CurrentPage,
            PerPage = readModels.PerPage,
            Count = readModels.Count,
            NumberOfPages = readModels.NumberOfPages
        };
    }
}
