#region ========================================================================= USING =====================================================================================
using Nemera.Application.Core.Favorites.Commands.AddFavorite;
using Nemera.Contracts.Requests.Favorites;
#endregion

namespace Nemera.Application.Common.Mapping.Favorites;

/// <summary>
/// Extension methods for converting <see cref="AddFavoriteRequest"/>.
/// </summary>
public static class AddFavoriteRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="AddFavoriteCommand"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted command.</returns>
    public static AddFavoriteCommand ToCommand(this AddFavoriteRequest request)
    {
        return new AddFavoriteCommand(request.MediaKind, request.MediaItemId);
    }
}
