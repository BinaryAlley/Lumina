#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoriteStatusRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusRequestTests
{
    private readonly GetFavoriteStatusRequestFixture _getFavoriteStatusRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusRequestTests"/> class.
    /// </summary>
    public GetFavoriteStatusRequestTests()
    {
        _getFavoriteStatusRequestFixture = new GetFavoriteStatusRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        GetFavoriteStatusRequest sut = _getFavoriteStatusRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.True(Enum.IsDefined(sut.MediaKind.Value));
        Assert.NotNull(sut.MediaItemIds);
        Assert.NotEmpty(sut.MediaItemIds);
        Assert.All(sut.MediaItemIds, mediaItemId => Assert.NotEqual(Guid.Empty, mediaItemId));
    }

    [Fact]
    public void Constructor_WhenMediaItemIdsAreSupplied_ShouldPreserveTheirOrderAndCount()
    {
        // Arrange
        Guid firstId = Guid.NewGuid();
        Guid secondId = Guid.NewGuid();
        Guid thirdId = Guid.NewGuid();
        List<Guid> mediaItemIds = [firstId, secondId, thirdId];

        // Act
        GetFavoriteStatusRequest sut = _getFavoriteStatusRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            mediaItemIds: mediaItemIds);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(FavoriteMediaKind.MusicTrack, sut.MediaKind);
        Assert.NotNull(sut.MediaItemIds);
        Assert.Equal(3, sut.MediaItemIds.Count);
        Assert.Equal<Guid>(mediaItemIds, sut.MediaItemIds);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Act
        GetFavoriteStatusRequest sut = _getFavoriteStatusRequestFixture.Create(includeMediaKind: false);

        // Assert
        Assert.Null(sut.MediaKind);
        Assert.NotNull(sut.MediaItemIds);
    }

    [Fact]
    public void Constructor_WhenMediaItemIdsAreNull_ShouldPreserveNull()
    {
        // Act
        GetFavoriteStatusRequest sut = _getFavoriteStatusRequestFixture.Create(includeMediaItemIds: false);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.Null(sut.MediaItemIds);
    }

    [Fact]
    public void Constructor_WhenMediaItemIdsAreEmpty_ShouldPreserveTheEmptyList()
    {
        // Act
        GetFavoriteStatusRequest sut = _getFavoriteStatusRequestFixture.Create(mediaItemIds: []);

        // Assert
        Assert.NotNull(sut.MediaItemIds);
        Assert.Empty(sut.MediaItemIds);
    }

    [Fact]
    public void Constructor_WhenMediaItemIdsContainTheEmptyGuid_ShouldPreserveIt()
    {
        // Arrange
        List<Guid> mediaItemIds = [Guid.NewGuid(), Guid.Empty];

        // Act
        GetFavoriteStatusRequest sut = _getFavoriteStatusRequestFixture.Create(mediaItemIds: mediaItemIds);

        // Assert
        Assert.NotNull(sut.MediaItemIds);
        Assert.Equal(2, sut.MediaItemIds.Count);
        Assert.Contains(Guid.Empty, sut.MediaItemIds);
        Assert.Equal<Guid>(
            mediaItemIds,
            sut.MediaItemIds);
    }
}
