#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using System;
#endregion

namespace Lumina.Application.Common.Mapping.Favorites;

/// <summary>
/// Extension methods for converting <see cref="FavoriteEntity"/>.
/// </summary>
public static class FavoriteEntityMapping
{
    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="Favorite"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully converted <see cref="Favorite"/>, or an error message.
    /// </returns>
    public static Result<Favorite> ToDomainEntity(this FavoriteEntity repositoryEntity)
    {
        return Favorite.Create(
            FavoriteId.Create(repositoryEntity.Id),
            UserId.Create(repositoryEntity.UserId),
            MediaItemId.Create(repositoryEntity.MediaItemId),
            repositoryEntity.MediaKind,
            repositoryEntity.CreatedOnUtc,
            Optional<DateTime>.FromNullable(repositoryEntity.UpdatedOnUtc));
    }

    /// <summary>
    /// Converts <paramref name="repositoryEntity"/> to <see cref="FavoriteResponse"/>.
    /// </summary>
    /// <param name="repositoryEntity">The repository entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static FavoriteResponse ToResponse(this FavoriteEntity repositoryEntity)
    {
        return new FavoriteResponse(
            repositoryEntity.Id,
            repositoryEntity.MediaKind,
            repositoryEntity.MediaItemId,
            repositoryEntity.CreatedOnUtc);
    }
}
