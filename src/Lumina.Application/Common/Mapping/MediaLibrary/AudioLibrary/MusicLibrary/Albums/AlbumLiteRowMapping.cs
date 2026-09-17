#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Extension methods for converting <see cref="AlbumLiteRow"/>.
/// </summary>
public static class AlbumLiteRowMapping
{
    /// <summary>
    /// Converts <paramref name="readModel"/> to <see cref="AlbumLiteResponse"/>.
    /// </summary>
    /// <param name="readModel">The read model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static AlbumLiteResponse ToResponse(this AlbumLiteRow readModel)
    {
        return new AlbumLiteResponse(
            readModel.Id,
            readModel.Title,
            readModel.TotalTracks);
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a collection of <see cref="AlbumLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The read models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<AlbumLiteResponse> ToResponses(this IEnumerable<AlbumLiteRow> readModels)
    {
        return [.. readModels.Select(readModel => readModel.ToResponse())];
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a paginated collection of <see cref="AlbumLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The paginated read models to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<AlbumLiteResponse> ToResponses(this PaginatedResultDto<AlbumLiteRow> readModels)
    {
        return new PaginatedResponse<AlbumLiteResponse>
        {
            Data = readModels.Data.ToResponses(),
            CurrentPage = readModels.CurrentPage,
            PerPage = readModels.PerPage,
            Count = readModels.Count,
            NumberOfPages = readModels.NumberOfPages
        };
    }
}
