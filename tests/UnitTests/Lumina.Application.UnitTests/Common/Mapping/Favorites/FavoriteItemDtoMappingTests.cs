#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DTO.Favorites;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Fixtures.Common.DTO.Favorites;
using Lumina.Application.Fixtures.Common.DTO.Pagination;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="FavoriteItemDtoMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoriteItemDtoMappingTests
{
    private readonly FavoriteEntityFixture _favoriteEntityFixture = new();
    private readonly FavoriteItemDtoFixture _favoriteItemDtoFixture = new();
    private readonly FavoriteItemDisplayDtoFixture _favoriteItemDisplayDtoFixture = new();
    private readonly PaginatedResultDtoFixture<FavoriteItemDto> _paginatedResultDtoFixture = new();

    [Fact]
    public void ToResponse_WhenDisplayIsMissing_ShouldReturnAnUnavailableItemWithEmptyDisplayFields()
    {
        // Arrange
        FavoriteEntity favorite = _favoriteEntityFixture.Create(mediaKind: FavoriteMediaKind.MusicTrack);
        FavoriteItemDto item = _favoriteItemDtoFixture.Create(favorite: favorite, includeDisplay: false);

        // Act
        FavoriteItemResponse response = item.ToResponse();

        // Assert
        Assert.Equal(favorite.MediaKind, response.MediaKind);
        Assert.Equal(favorite.MediaItemId, response.MediaItemId);
        Assert.False(response.IsAvailable);
        Assert.Equal(Guid.Empty, response.LibraryId);
        Assert.Equal(string.Empty, response.Title);
        Assert.Equal(Guid.Empty, response.CollectionId);
        Assert.Equal(string.Empty, response.CollectionTitle);
        Assert.Equal(Guid.Empty, response.CreatorId);
        Assert.Equal(string.Empty, response.CreatorName);
        Assert.Null(response.CoverPath);
    }

    [Fact]
    public void ToResponse_WhenDisplayIsPresent_ShouldMapEveryDisplayField()
    {
        // Arrange
        FavoriteEntity favorite = _favoriteEntityFixture.Create(mediaKind: FavoriteMediaKind.MusicAlbum);
        FavoriteItemDisplayDto display = _favoriteItemDisplayDtoFixture.Create();
        FavoriteItemDto item = _favoriteItemDtoFixture.Create(favorite: favorite, display: display);

        // Act
        FavoriteItemResponse response = item.ToResponse();

        // Assert
        Assert.Equal(favorite.MediaKind, response.MediaKind);
        Assert.Equal(favorite.MediaItemId, response.MediaItemId);
        Assert.True(response.IsAvailable);
        Assert.Equal(display.LibraryId, response.LibraryId);
        Assert.Equal(display.Title, response.Title);
        Assert.Equal(display.CollectionId, response.CollectionId);
        Assert.Equal(display.CollectionTitle, response.CollectionTitle);
        Assert.Equal(display.CreatorId, response.CreatorId);
        Assert.Equal(display.CreatorName, response.CreatorName);
        Assert.Equal(display.CoverPath, response.CoverPath);
    }

    [Fact]
    public void ToResponse_WhenDisplayHasNoCoverPath_ShouldMapNullCoverPath()
    {
        // Arrange
        FavoriteItemDisplayDto display = _favoriteItemDisplayDtoFixture.Create(includeCoverPath: false);
        FavoriteItemDto item = _favoriteItemDtoFixture.Create(display: display);

        // Act
        FavoriteItemResponse response = item.ToResponse();

        // Assert
        Assert.True(response.IsAvailable);
        Assert.Null(response.CoverPath);
    }

    [Fact]
    public void ToResponses_WhenMappingAPage_ShouldPreserveTheOrderOfTheItemsAndThePaginationMetadata()
    {
        // Arrange
        FavoriteItemDto firstItem = _favoriteItemDtoFixture.Create(favorite: _favoriteEntityFixture.Create());
        FavoriteItemDto secondItem = _favoriteItemDtoFixture.Create(favorite: _favoriteEntityFixture.Create());
        FavoriteItemDto thirdItem = _favoriteItemDtoFixture.Create(favorite: _favoriteEntityFixture.Create(), includeDisplay: false);
        List<FavoriteItemDto> items = [firstItem, secondItem, thirdItem];
        PaginatedResultDto<FavoriteItemDto> page = _paginatedResultDtoFixture.Create(
            data: items,
            currentPage: 2,
            perPage: 3,
            count: 8,
            numberOfPages: 3);

        // Act
        PaginatedResponse<FavoriteItemResponse> response = page.ToResponses();

        // Assert
        Assert.Equal(3, response.Data.Count);
        Assert.Equal(
            items.Select(item => item.Favorite.MediaItemId),
            response.Data.Select(item => item.MediaItemId));
        Assert.True(response.Data[0].IsAvailable);
        Assert.True(response.Data[1].IsAvailable);
        Assert.False(response.Data[2].IsAvailable);
        Assert.Equal(2, response.CurrentPage);
        Assert.Equal(3, response.PerPage);
        Assert.Equal(8, response.Count);
        Assert.Equal(3, response.NumberOfPages);
    }
}
