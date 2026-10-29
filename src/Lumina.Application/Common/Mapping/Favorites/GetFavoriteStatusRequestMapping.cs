#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Contracts.Requests.Favorites;
#endregion

namespace Lumina.Application.Common.Mapping.Favorites;

/// <summary>
/// Extension methods for converting <see cref="GetFavoriteStatusRequest"/>.
/// </summary>
public static class GetFavoriteStatusRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetFavoriteStatusQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetFavoriteStatusQuery ToQuery(this GetFavoriteStatusRequest request)
    {
        return new GetFavoriteStatusQuery(request.MediaKind, request.MediaItemIds);
    }
}
