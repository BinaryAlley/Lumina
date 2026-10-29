#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoriteStatusRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusRequestMappingTests
{
    private readonly GetFavoriteStatusRequestFixture _getFavoriteStatusRequestFixture = new();

    [Fact]
    public void ToQuery_WhenEveryMemberIsSupplied_ShouldPreserveTheKindAndTheMediaItemIdOrder()
    {
        // Arrange
        Guid firstId = Guid.NewGuid();
        Guid secondId = Guid.NewGuid();
        Guid thirdId = Guid.NewGuid();
        List<Guid> mediaItemIds = [firstId, secondId, thirdId];
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicAlbum,
            mediaItemIds: mediaItemIds);

        // Act
        GetFavoriteStatusQuery query = request.ToQuery();

        // Assert
        Assert.Equal(FavoriteMediaKind.MusicAlbum, query.MediaKind);
        Assert.NotNull(query.MediaItemIds);
        Assert.Equal<Guid>(mediaItemIds, query.MediaItemIds);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(includeMediaKind: false);

        // Act
        GetFavoriteStatusQuery query = request.ToQuery();

        // Assert
        Assert.Null(query.MediaKind);
        Assert.NotNull(query.MediaItemIds);
    }

    [Fact]
    public void ToQuery_WhenMediaItemIdsAreNull_ShouldPreserveNull()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(includeMediaItemIds: false);

        // Act
        GetFavoriteStatusQuery query = request.ToQuery();

        // Assert
        Assert.NotNull(query.MediaKind);
        Assert.Null(query.MediaItemIds);
    }

    [Fact]
    public void ToQuery_WhenMediaItemIdsAreEmpty_ShouldPreserveTheEmptyList()
    {
        // Arrange
        GetFavoriteStatusRequest request = _getFavoriteStatusRequestFixture.Create(mediaItemIds: []);

        // Act
        GetFavoriteStatusQuery query = request.ToQuery();

        // Assert
        Assert.NotNull(query.MediaItemIds);
        Assert.Empty(query.MediaItemIds);
    }
}
