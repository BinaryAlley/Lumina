#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtists;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistsRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistsRequestMappingTests
{
    private readonly GetArtistsRequestFixture _getArtistsRequestFixture = new();

    [Fact]
    public void ToQuery_WhenCalledWithRouteId_ShouldMapRouteIdFromArgument()
    {
        // Arrange
        GetArtistsRequest request = _getArtistsRequestFixture.Create();

        // Act
        GetArtistsQuery result = request.ToQuery("library-route");

        // Assert
        Assert.Equal("library-route", result.LibraryId);
    }

    [Fact]
    public void ToQuery_WhenSearchTermIsProvided_ShouldMapIt()
    {
        // Arrange
        GetArtistsRequest request = _getArtistsRequestFixture.Create(searchTerm: "queen");

        // Act
        GetArtistsQuery result = request.ToQuery("library-route");

        // Assert
        Assert.Equal("queen", result.SearchTerm);
    }

    [Fact]
    public void ToQuery_WhenNoPaginationValuesAreProvided_ShouldMapNullPaginationData()
    {
        // Arrange
        GetArtistsRequest request = _getArtistsRequestFixture.Create(includeCurrentPage: false, includePerPage: false);

        // Act
        GetArtistsQuery result = request.ToQuery("library-route");

        // Assert
        Assert.Null(result.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsProvided_ShouldDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetArtistsRequest request = _getArtistsRequestFixture.Create(currentPage: 3, includePerPage: false);

        // Act
        GetArtistsQuery result = request.ToQuery("library-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(3, result.PaginationData.CurrentPage);
        Assert.Equal(200, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsProvided_ShouldDefaultCurrentPageToOne()
    {
        // Arrange
        GetArtistsRequest request = _getArtistsRequestFixture.Create(perPage: 50, includeCurrentPage: false);

        // Act
        GetArtistsQuery result = request.ToQuery("library-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(1, result.PaginationData.CurrentPage);
        Assert.Equal(50, result.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreProvided_ShouldMapThem()
    {
        // Arrange
        GetArtistsRequest request = _getArtistsRequestFixture.Create(currentPage: 2, perPage: 25);

        // Act
        GetArtistsQuery result = request.ToQuery("library-route");

        // Assert
        Assert.NotNull(result.PaginationData);
        Assert.Equal(2, result.PaginationData.CurrentPage);
        Assert.Equal(25, result.PaginationData.PerPage);
    }
}
