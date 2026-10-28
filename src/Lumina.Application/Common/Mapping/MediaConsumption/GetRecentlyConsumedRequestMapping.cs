#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Core.MediaConsumption.Queries.GetRecentlyConsumed;
using Lumina.Contracts.Requests.MediaConsumption;
#endregion

namespace Lumina.Application.Common.Mapping.MediaConsumption;

/// <summary>
/// Extension methods for converting <see cref="GetRecentlyConsumedRequest"/>.
/// </summary>
public static class GetRecentlyConsumedRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetRecentlyConsumedQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <param name="targetUserId">The Id of the user whose history is retrieved, taken from the route, or <see langword="null"/> for the current user.</param>
    /// <returns>The converted query.</returns>
    public static GetRecentlyConsumedQuery ToQuery(this GetRecentlyConsumedRequest request, string? targetUserId)
    {
        PaginationDataDto? paginationData = null;
        if (request.CurrentPage is not null || request.PerPage is not null)
            paginationData = new PaginationDataDto
            {
                CurrentPage = request.CurrentPage ?? 1,
                PerPage = request.PerPage ?? 50
            };

        return new GetRecentlyConsumedQuery(
            targetUserId,
            request.LibraryId,
            request.MediaKind,
            request.FromUtc,
            request.ToUtc,
            paginationData
        );
    }
}
