#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.Favorites;

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
            MediaItemId = domainEntity.MediaItemId.Value,
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            UpdatedOnUtc = domainEntity.UpdatedOnUtc.HasValue ? domainEntity.UpdatedOnUtc.Value : null,
            UpdatedBy = null
        };
    }
}
