#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Extension methods for converting <see cref="ArtistLiteRow"/>.
/// </summary>
public static class ArtistLiteRowMapping
{
    /// <summary>
    /// Converts <paramref name="readModel"/> to <see cref="ArtistLiteResponse"/>.
    /// </summary>
    /// <param name="readModel">The read model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ArtistLiteResponse ToResponse(this ArtistLiteRow readModel)
    {
        return new ArtistLiteResponse(
            readModel.Id,
            readModel.Name);
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a collection of <see cref="ArtistLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The read models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<ArtistLiteResponse> ToResponses(this IEnumerable<ArtistLiteRow> readModels)
    {
        return [.. readModels.Select(readModel => readModel.ToResponse())];
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a paginated collection of <see cref="ArtistLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The paginated read models to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<ArtistLiteResponse> ToResponses(this PaginatedResultDto<ArtistLiteRow> readModels)
    {
        return new PaginatedResponse<ArtistLiteResponse>
        {
            Data = readModels.Data.ToResponses(),
            CurrentPage = readModels.CurrentPage,
            PerPage = readModels.PerPage,
            Count = readModels.Count,
            NumberOfPages = readModels.NumberOfPages
        };
    }
}
