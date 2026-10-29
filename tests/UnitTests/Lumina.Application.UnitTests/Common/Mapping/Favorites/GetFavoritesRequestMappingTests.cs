#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Favorites;
using Lumina.Application.Core.Favorites.Queries.GetFavorites;
using Lumina.Contracts.Fixtures.Core.Requests.Favorites;
using Lumina.Contracts.Requests.Favorites;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Favorites;

/// <summary>
/// Contains unit tests for the <see cref="GetFavoritesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesRequestMappingTests
{
    private readonly GetFavoritesRequestFixture _getFavoritesRequestFixture = new();

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreOmitted_ShouldLeaveThePaginationDataNullAndPreserveTheMediaKind()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            mediaKind: FavoriteMediaKind.MusicArtist,
            includeCurrentPage: false,
            includePerPage: false);

        // Act
        GetFavoritesQuery query = request.ToQuery();

        // Assert
        Assert.Equal(FavoriteMediaKind.MusicArtist, query.MediaKind);
        Assert.Null(query.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsSupplied_ShouldPreserveItAndDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            currentPage: 42,
            includePerPage: false);

        // Act
        GetFavoritesQuery query = request.ToQuery();

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(42, query.PaginationData.CurrentPage);
        Assert.Equal(200, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsSupplied_ShouldPreserveItAndDefaultCurrentPageToOne()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(
            perPage: 7,
            includeCurrentPage: false);

        // Act
        GetFavoritesQuery query = request.ToQuery();

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(1, query.PaginationData.CurrentPage);
        Assert.Equal(7, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreSupplied_ShouldPreserveThem()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(currentPage: 3, perPage: 25);

        // Act
        GetFavoritesQuery query = request.ToQuery();

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(3, query.PaginationData.CurrentPage);
        Assert.Equal(25, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenPaginationValuesAreBelowOne_ShouldClampThemToOne()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(currentPage: 0, perPage: -5);

        // Act
        GetFavoritesQuery query = request.ToQuery();

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(1, query.PaginationData.CurrentPage);
        Assert.Equal(1, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenMediaKindIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetFavoritesRequest request = _getFavoritesRequestFixture.Create(includeMediaKind: false);

        // Act
        GetFavoritesQuery query = request.ToQuery();

        // Assert
        Assert.Null(query.MediaKind);
        Assert.NotNull(query.PaginationData);
    }
}
