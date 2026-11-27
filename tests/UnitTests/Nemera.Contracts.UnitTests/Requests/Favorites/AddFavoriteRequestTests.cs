#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="AddFavoriteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteRequestTests
{
    private readonly AddFavoriteRequestFixture _addFavoriteRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteRequestTests"/> class.
    /// </summary>
    public AddFavoriteRequestTests()
    {
        _addFavoriteRequestFixture = new AddFavoriteRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        AddFavoriteRequest sut = _addFavoriteRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.True(Enum.IsDefined(sut.MediaKind.Value));
        Assert.NotNull(sut.MediaItemId);
        Assert.NotEqual(Guid.Empty, sut.MediaItemId.Value);
    }

    [Fact]
    public void Constructor_WhenEveryMemberIsSupplied_ShouldPreserveThem()
    {
        // Arrange
        Guid mediaItemId = Guid.NewGuid();

        // Act
        AddFavoriteRequest sut = _addFavoriteRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicAlbum,
            mediaItemId: mediaItemId);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(FavoriteMediaKind.MusicAlbum, sut.MediaKind);
        Assert.NotNull(sut.MediaItemId);
        Assert.Equal(mediaItemId, sut.MediaItemId);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Act
        AddFavoriteRequest sut = _addFavoriteRequestFixture.Create(includeMediaKind: false);

        // Assert
        Assert.Null(sut.MediaKind);
        Assert.NotNull(sut.MediaItemId);
    }

    [Fact]
    public void Constructor_WhenMediaItemIdIsNull_ShouldPreserveNull()
    {
        // Act
        AddFavoriteRequest sut = _addFavoriteRequestFixture.Create(includeMediaItemId: false);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.Null(sut.MediaItemId);
    }

    [Fact]
    public void Constructor_WhenMediaItemIdIsEmpty_ShouldPreserveEmpty()
    {
        // Act
        AddFavoriteRequest sut = _addFavoriteRequestFixture.Create(mediaItemId: Guid.Empty);

        // Assert
        Assert.NotNull(sut.MediaItemId);
        Assert.Equal(Guid.Empty, sut.MediaItemId);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsUndefined_ShouldPreserveTheUndefinedValue()
    {
        // Arrange
        FavoriteMediaKind mediaKind = (FavoriteMediaKind)int.MaxValue;

        // Act
        AddFavoriteRequest sut = _addFavoriteRequestFixture.Create(mediaKind: mediaKind);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(mediaKind, sut.MediaKind);
    }
}
