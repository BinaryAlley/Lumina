#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Favorites;
using Nemera.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
#endregion

namespace Nemera.Application.Common.Mapping.Favorites;

/// <summary>
/// Extension methods for converting <see cref="Favorite"/>.
/// </summary>
public static class FavoriteMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="FavoriteEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static FavoriteEntity ToRepositoryEntity(this Favorite domainEntity)
    {
        return new FavoriteEntity
        {
            Id = domainEntity.Id.Value,
            UserId = domainEntity.UserId.Value,
            MediaKind = domainEntity.MediaKind,
            MediaItemId = domainEntity.MediaItemId.Value
        };
    }
}
