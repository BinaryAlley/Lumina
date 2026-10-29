#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.MediaLibraryBoundedContext.MediaItemAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.ExternalIdentifiers.UserManagementBoundedContext.UserAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate;
using Lumina.Domain.Fixtures.Core.BoundedContexts.FavoriteBoundedContext.FavoriteAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Favorites;

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
    public void ToRepositoryEntity_WhenUpdatedOnUtcIsPresent_ShouldMapEveryFieldAndTheAuditColumns()
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
        Assert.Equal(createdOnUtc, entity.CreatedOnUtc);
        Assert.Equal(Guid.Empty, entity.CreatedBy);
        Assert.True(entity.UpdatedOnUtc.HasValue);
        Assert.Equal(updatedOnUtc, entity.UpdatedOnUtc.Value);
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
