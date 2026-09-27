#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Extension methods for converting <see cref="TrackLiteRow"/>.
/// </summary>
public static class TrackLiteRowMapping
{
    /// <summary>
    /// Converts <paramref name="readModel"/> to <see cref="TrackLiteResponse"/>.
    /// </summary>
    /// <param name="readModel">The read model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static TrackLiteResponse ToResponse(this TrackLiteRow readModel)
    {
        return new TrackLiteResponse(
            readModel.Id,
            readModel.Title,
            readModel.TrackNumber,
            readModel.DiscNumber);
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a collection of <see cref="TrackLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The read models to be converted.</param>
    /// <returns>The converted responses.</returns>
    public static IReadOnlyList<TrackLiteResponse> ToResponses(this IEnumerable<TrackLiteRow> readModels)
    {
        return [.. readModels.Select(readModel => readModel.ToResponse())];
    }

    /// <summary>
    /// Converts <paramref name="readModels"/> to a paginated collection of <see cref="TrackLiteResponse"/>.
    /// </summary>
    /// <param name="readModels">The paginated read models to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<TrackLiteResponse> ToResponses(this PaginatedResultDto<TrackLiteRow> readModels)
    {
        return new PaginatedResponse<TrackLiteResponse>
        {
            Data = readModels.Data.ToResponses(),
            CurrentPage = readModels.CurrentPage,
            PerPage = readModels.PerPage,
            Count = readModels.Count,
            NumberOfPages = readModels.NumberOfPages
        };
    }
}
