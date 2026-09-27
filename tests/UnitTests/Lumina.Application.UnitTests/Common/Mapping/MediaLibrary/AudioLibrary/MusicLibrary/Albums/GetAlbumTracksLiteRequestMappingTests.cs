#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetAlbumTracksLite;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Contains unit tests for the <see cref="GetAlbumTracksLiteRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksLiteRequestMappingTests
{
    private readonly GetAlbumTracksLiteRequestFixture _getAlbumTracksLiteRequestFixture = new();

    [Fact]
    public void ToQuery_WhenCalledWithRouteIds_ShouldMapRouteIdsFromArguments()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create();

        // Act
        GetAlbumTracksLiteQuery result = request.ToQuery("library-route", "artist-route", "album-route");

        // Assert
        Assert.Equal("library-route", result.LibraryId);
        Assert.Equal("artist-route", result.ArtistId);
        Assert.Equal("album-route", result.AlbumId);
    }

    [Fact]
    public void ToQuery_WhenNoPaginationValuesAreProvided_ShouldMapNullPaginationData()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(includeCurrentPage: false, includePerPage: false);

        // Act
        GetAlbumTracksLiteQuery result = request.ToQuery("library-route", "artist-route", "album-route");

        // Assert
        Assert.Null(result.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(currentPage: 3, includePerPage: false);

        // Act
        GetAlbumTracksLiteQuery result = request.ToQuery("library-route", "artist-route", "album-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(perPage: 50, includeCurrentPage: false);

        // Act
        GetAlbumTracksLiteQuery result = request.ToQuery("library-route", "artist-route", "album-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData.CurrentPage);
        Assert.Equal(50, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreProvided_ShouldMapThem()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(currentPage: 2, perPage: 25);

        // Act
        GetAlbumTracksLiteQuery result = request.ToQuery("library-route", "artist-route", "album-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(2, result.PaginationData.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }
}
