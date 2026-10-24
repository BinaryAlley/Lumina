#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaConsumption;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Utilities;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="ConsumedItemDto"/>.
/// </summary>
public static class ConsumedItemDtoMapping
{
    /// <summary>
    /// Converts <paramref name="data"/> to <see cref="ConsumedItemResponse"/>.
    /// </summary>
    /// <param name="data">The data transfer object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ConsumedItemResponse ToResponse(this ConsumedItemDto data)
    {
        return new ConsumedItemResponse(
            data.MediaKind,
            data.MediaItemId,
            data.LibraryId,
            data.IsAvailable,
            data.Title,
            data.CollectionTitle,
            data.CreatorName,
            data.CoverPath,
            DateTime.SpecifyKind(data.LastConsumedOnUtc, DateTimeKind.Utc),
            data.CountedSessionCount,
            data.CompletedSessionCount,
            data.ConsumedDurationMilliseconds.ToClampedTimeSpan().TotalSeconds
        );
    }

    /// <summary>
    /// Converts <paramref name="data"/> to a paginated collection of <see cref="ConsumedItemResponse"/>.
    /// </summary>
    /// <param name="data">The paginated data transfer objects to be converted.</param>
    /// <returns>The converted paginated responses.</returns>
    public static PaginatedResponse<ConsumedItemResponse> ToResponses(this PaginatedResultDto<ConsumedItemDto> data)
    {
        return new PaginatedResponse<ConsumedItemResponse>
        {
            Data = [.. data.Data.Select(item => item.ToResponse())],
            CurrentPage = data.CurrentPage,
            PerPage = data.PerPage,
            Count = data.Count,
            NumberOfPages = data.NumberOfPages
        };
    }
}
