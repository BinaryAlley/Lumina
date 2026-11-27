#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.Favorites;
using Nemera.Application.Common.Mapping.Favorites;
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Nemera.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Nemera.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Nemera.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Nemera.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="FavoriteMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteMappingTests
{
    private readonly FavoriteFixture _favoriteFixture = new();
    private readonly FavoriteIdFixture _favoriteIdFixture = new();
    private readonly UserIdFixture _userIdFixture = new();
    private readonly MediaItemIdFixture _mediaItemIdFixture = new();

    [Fact]
    public void ToRepositoryEntity_WhenUpdatedOnUtcIsPresent_ShouldMapEveryFieldWithoutTheAuditColumns()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime createdOnUtc = new(2024, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 2, 4, 5, 6, 7, DateTimeKind.Utc);
        Favorite favorite = _favoriteFixture.Create(
            id: _favoriteIdFixture.Create(id),
            userId: _userIdFixture.Create(userId),
            mediaItemId: _mediaItemIdFixture.Create(mediaItemId),
            mediaKind: FavoriteMediaKind.Book,
            createdOnUtc: createdOnUtc,
            updatedOnUtc: Optional<DateTime>.Some(updatedOnUtc));

        // Act
        FavoriteEntity entity = favorite.ToRepositoryEntity();

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal(userId, entity.UserId);
        Assert.Equal(FavoriteMediaKind.Book, entity.MediaKind);
        Assert.Equal(mediaItemId, entity.MediaItemId);
        Assert.Equal(default, entity.CreatedOnUtc);
        Assert.Equal(Guid.Empty, entity.CreatedBy);
        Assert.Null(entity.UpdatedOnUtc);
        Assert.Null(entity.UpdatedBy);
    }

    [Fact]
    public void ToRepositoryEntity_WhenUpdatedOnUtcIsMissing_ShouldMapNullUpdatedOnUtc()
    {
        // Arrange
        Favorite favorite = _favoriteFixture.Create(includeUpdatedOnUtc: false);

        // Act
        FavoriteEntity entity = favorite.ToRepositoryEntity();

        // Assert
        Assert.Null(entity.UpdatedOnUtc);
        Assert.Null(entity.UpdatedBy);
        Assert.Equal(Guid.Empty, entity.CreatedBy);
    }
}
