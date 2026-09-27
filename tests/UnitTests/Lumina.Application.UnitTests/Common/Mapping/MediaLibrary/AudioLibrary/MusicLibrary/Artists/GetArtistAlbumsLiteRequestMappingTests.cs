#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistAlbumsLiteRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsLiteRequestMappingTests
{
    private readonly GetArtistAlbumsLiteRequestFixture _getArtistAlbumsLiteRequestFixture = new();

    [Fact]
    public void ToQuery_WhenCalledWithRouteIds_ShouldMapRouteIdsFromArguments()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();

        // Act
        GetArtistAlbumsLiteQuery result = request.ToQuery("library-route", "artist-route");

        // Assert
        Assert.Equal("library-route", result.LibraryId);
        Assert.Equal("artist-route", result.ArtistId);
    }

    [Fact]
    public void ToQuery_WhenNoPaginationValuesAreProvided_ShouldMapNullPaginationData()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(includeCurrentPage: false, includePerPage: false);

        // Act
        GetArtistAlbumsLiteQuery result = request.ToQuery("library-route", "artist-route");

        // Assert
        Assert.Null(result.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(currentPage: 3, includePerPage: false);

        // Act
        GetArtistAlbumsLiteQuery result = request.ToQuery("library-route", "artist-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(perPage: 50, includeCurrentPage: false);

        // Act
        GetArtistAlbumsLiteQuery result = request.ToQuery("library-route", "artist-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData.CurrentPage);
        Assert.Equal(50, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreProvided_ShouldMapThem()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(currentPage: 2, perPage: 25);

        // Act
        GetArtistAlbumsLiteQuery result = request.ToQuery("library-route", "artist-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(2, result.PaginationData.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }
}
