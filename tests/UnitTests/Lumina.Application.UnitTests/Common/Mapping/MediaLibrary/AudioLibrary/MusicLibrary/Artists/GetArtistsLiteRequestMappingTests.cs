#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistsLiteRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsLiteRequestMappingTests
{
    private readonly GetArtistsLiteRequestFixture _getArtistsLiteRequestFixture = new();

    [Fact]
    public void ToQuery_WhenCalledWithRouteId_ShouldMapRouteIdFromArgument()
    {
        // Arrange
        GetArtistsLiteRequest request = _getArtistsLiteRequestFixture.Create();

        // Act
        GetArtistsLiteQuery result = request.ToQuery("library-route");

        // Assert
        Assert.Equal("library-route", result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenSearchTermIsProvided_ShouldMapIt()
    {
        // Arrange
        GetArtistsLiteRequest request = _getArtistsLiteRequestFixture.Create(searchTerm: "queen");

        // Act
        GetArtistsLiteQuery result = request.ToQuery("library-route");

        // Assert
        Assert.Equal("queen", result.SearchTerm);
    }

    [Fact]
    public void ToQuery_WhenNoPaginationValuesAreProvided_ShouldMapNullPaginationData()
    {
        // Arrange
        GetArtistsLiteRequest request = _getArtistsLiteRequestFixture.Create(includeCurrentPage: false, includePerPage: false);

        // Act
        GetArtistsLiteQuery result = request.ToQuery("library-route");

        // Assert
        Assert.Null(result.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetArtistsLiteRequest request = _getArtistsLiteRequestFixture.Create(currentPage: 3, includePerPage: false);

        // Act
        GetArtistsLiteQuery result = request.ToQuery("library-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetArtistsLiteRequest request = _getArtistsLiteRequestFixture.Create(perPage: 50, includeCurrentPage: false);

        // Act
        GetArtistsLiteQuery result = request.ToQuery("library-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData.CurrentPage);
        Assert.Equal(50, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreProvided_ShouldMapThem()
    {
        // Arrange
        GetArtistsLiteRequest request = _getArtistsLiteRequestFixture.Create(currentPage: 2, perPage: 25);

        // Act
        GetArtistsLiteQuery result = request.ToQuery("library-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(2, result.PaginationData.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }
}
