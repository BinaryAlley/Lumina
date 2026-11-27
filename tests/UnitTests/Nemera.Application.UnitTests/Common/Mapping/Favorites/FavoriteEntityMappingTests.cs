#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="FavoriteEntityMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteEntityMappingTests
{
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();

    [Fact]
    public void ToDomainEntity_WhenUpdatedOnUtcIsPresent_ShouldMapEveryFieldAndTheUpdateTime()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime createdOnUtc = new(2024, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        DateTime updatedOnUtc = new(2024, 3, 2, 11, 30, 0, DateTimeKind.Utc);
        FavoriteEntity entity = _favoriteEntityFixture.Create(
            id: id,
            userId: userId,
            mediaKind: FavoriteMediaKind.MusicAlbum,
            mediaItemId: mediaItemId,
            createdOnUtc: createdOnUtc,
            updatedOnUtc: updatedOnUtc);

        // Act
        Result<Favorite> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Favorite favorite = result.Value;
        Assert.Equal(id, favorite.Id.Value);
        Assert.Equal(userId, favorite.UserId.Value);
        Assert.Equal(mediaItemId, favorite.MediaItemId.Value);
        Assert.Equal(FavoriteMediaKind.MusicAlbum, favorite.MediaKind);
        Assert.Equal(createdOnUtc, favorite.CreatedOnUtc);
        Assert.True(favorite.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, favorite.UpdatedOnUtc.Value);
        Assert.False(favorite.IsRemoved);
    }

    [Fact]
    public void ToDomainEntity_WhenUpdatedOnUtcIsMissing_ShouldMapNoUpdateTime()
    {
        // Arrange
        FavoriteEntity entity = _favoriteEntityFixture.Create(includeUpdatedOnUtc: false);

        // Act
        Result<Favorite> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.UpdatedOnUtc.HasValue);
    }

    [Fact]
    public void ToDomainEntity_WhenTheStoredMediaKindIsNotSupportedToday_ShouldStillRehydrateIt()
    {
        // Arrange
        FavoriteEntity entity = _favoriteEntityFixture.Create(mediaKind: FavoriteMediaKind.Video);

        // Act
        Result<Favorite> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FavoriteMediaKind.Video, result.Value.MediaKind);
    }

    [Fact]
    public void ToDomainEntity_WhenMappingTheId_ShouldKeepTheSameIdentifier()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        FavoriteEntity entity = _favoriteEntityFixture.Create(id: id);

        // Act
        Result<Favorite> result = entity.ToDomainEntity();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id.Value);
    }

    [Fact]
    public void ToResponse_WhenMappingTheEntity_ShouldMapEveryField()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        DateTime createdOnUtc = new(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc);
        FavoriteEntity entity = _favoriteEntityFixture.Create(
            id: id,
            mediaKind: FavoriteMediaKind.Book,
            mediaItemId: mediaItemId,
            createdOnUtc: createdOnUtc);

        // Act
        FavoriteResponse response = entity.ToResponse();

        // Assert
        Assert.Equal(id, response.Id);
        Assert.Equal(FavoriteMediaKind.Book, response.MediaKind);
        Assert.Equal(mediaItemId, response.MediaItemId);
        Assert.Equal(createdOnUtc, response.FavoritedOnUtc);
    }
}
