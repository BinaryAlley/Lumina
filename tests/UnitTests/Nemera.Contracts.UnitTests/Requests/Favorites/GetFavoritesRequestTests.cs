#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoritesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesRequestTests
{
    private readonly GetFavoritesRequestFixture _getFavoritesRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesRequestTests"/> class.
    /// </summary>
    public GetFavoritesRequestTests()
    {
        _getFavoritesRequestFixture = new GetFavoritesRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.True(Enum.IsDefined(sut.MediaKind.Value));
        Assert.NotNull(sut.CurrentPage);
        Assert.True(sut.CurrentPage.Value >= 1);
        Assert.NotNull(sut.PerPage);
        Assert.True(sut.PerPage.Value >= 1);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsSupplied_ShouldPreserveItIndependently()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create(
            mediaKind: FavoriteMediaKind.Book,
            currentPage: 4,
            perPage: 12);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(FavoriteMediaKind.Book, sut.MediaKind);
        Assert.NotNull(sut.CurrentPage);
        Assert.Equal(4, sut.CurrentPage);
        Assert.NotNull(sut.PerPage);
        Assert.Equal(12, sut.PerPage);
    }

    [Fact]
    public void Constructor_WhenCurrentPageIsSupplied_ShouldPreserveItIndependently()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicArtist,
            currentPage: 9,
            perPage: 30);

        // Assert
        Assert.NotNull(sut.CurrentPage);
        Assert.Equal(9, sut.CurrentPage);
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(FavoriteMediaKind.MusicArtist, sut.MediaKind);
        Assert.NotNull(sut.PerPage);
        Assert.Equal(30, sut.PerPage);
    }

    [Fact]
    public void Constructor_WhenPerPageIsSupplied_ShouldPreserveItIndependently()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            currentPage: 6,
            perPage: 60);

        // Assert
        Assert.NotNull(sut.PerPage);
        Assert.Equal(60, sut.PerPage);
        Assert.NotNull(sut.MediaKind);
        Assert.Equal(FavoriteMediaKind.MusicTrack, sut.MediaKind);
        Assert.NotNull(sut.CurrentPage);
        Assert.Equal(6, sut.CurrentPage);
    }

    [Fact]
    public void Constructor_WhenMediaKindIsNull_ShouldPreserveNullWhileKeepingThePagination()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create(includeMediaKind: false);

        // Assert
        Assert.Null(sut.MediaKind);
        Assert.NotNull(sut.CurrentPage);
        Assert.NotNull(sut.PerPage);
    }

    [Fact]
    public void Constructor_WhenCurrentPageIsNull_ShouldPreserveNullWhileKeepingTheOtherMembers()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create(includeCurrentPage: false);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.Null(sut.CurrentPage);
        Assert.NotNull(sut.PerPage);
    }

    [Fact]
    public void Constructor_WhenPerPageIsNull_ShouldPreserveNullWhileKeepingTheOtherMembers()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create(includePerPage: false);

        // Assert
        Assert.NotNull(sut.MediaKind);
        Assert.NotNull(sut.CurrentPage);
        Assert.Null(sut.PerPage);
    }

    [Fact]
    public void Constructor_WhenPaginationValuesAreBelowOne_ShouldPreserveThemAtTheContractLevel()
    {
        // Act
        GetFavoritesRequest sut = _getFavoritesRequestFixture.Create(currentPage: 0, perPage: -5);

        // Assert
        Assert.NotNull(sut.CurrentPage);
        Assert.Equal(0, sut.CurrentPage);
        Assert.NotNull(sut.PerPage);
        Assert.Equal(-5, sut.PerPage);
    }
}
