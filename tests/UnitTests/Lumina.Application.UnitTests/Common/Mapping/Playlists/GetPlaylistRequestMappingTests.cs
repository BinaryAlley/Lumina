#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Queries.GetPlaylist;
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistRequestMappingTests
{
    private readonly GetPlaylistRequestFixture _getPlaylistRequestFixture = new();

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreOmitted_ShouldLeaveThePaginationDataNullAndPreserveThePlaylistId()
    {
        // Arrange
        string playlistId = Guid.NewGuid().ToString();
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(
            includeCurrentPage: false,
            includePerPage: false);

        // Act
        GetPlaylistQuery query = request.ToQuery(playlistId);

        // Assert
        Assert.Equal(playlistId, query.PlaylistId);
        Assert.Null(query.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenOnlyCurrentPageIsSupplied_ShouldPreserveItAndDefaultPerPageToTwoHundred()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(
            currentPage: 42,
            includePerPage: false);
        string playlistId = Guid.NewGuid().ToString();

        // Act
        GetPlaylistQuery query = request.ToQuery(playlistId);

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(42, query.PaginationData.CurrentPage);
        Assert.Equal(200, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenOnlyPerPageIsSupplied_ShouldPreserveItAndDefaultCurrentPageToOne()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(
            perPage: 7,
            includeCurrentPage: false);
        string playlistId = Guid.NewGuid().ToString();

        // Act
        GetPlaylistQuery query = request.ToQuery(playlistId);

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(1, query.PaginationData.CurrentPage);
        Assert.Equal(7, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenBothPaginationValuesAreSupplied_ShouldPreserveThem()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(currentPage: 3, perPage: 25);
        string playlistId = Guid.NewGuid().ToString();

        // Act
        GetPlaylistQuery query = request.ToQuery(playlistId);

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(3, query.PaginationData.CurrentPage);
        Assert.Equal(25, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenPaginationValuesAreBelowOne_ShouldClampThemToOne()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create(currentPage: 0, perPage: -5);
        string playlistId = Guid.NewGuid().ToString();

        // Act
        GetPlaylistQuery query = request.ToQuery(playlistId);

        // Assert
        Assert.NotNull(query.PaginationData);
        Assert.Equal(1, query.PaginationData.CurrentPage);
        Assert.Equal(1, query.PaginationData.PerPage);
    }

    [Fact]
    public void ToQuery_WhenPlaylistIdIsNull_ShouldPreserveNull()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();

        // Act
        GetPlaylistQuery query = request.ToQuery(null);

        // Assert
        Assert.Null(query.PlaylistId);
        Assert.NotNull(query.PaginationData);
    }

    [Fact]
    public void ToQuery_WhenPlaylistIdIsEmpty_ShouldPreserveEmpty()
    {
        // Arrange
        GetPlaylistRequest request = _getPlaylistRequestFixture.Create();

        // Act
        GetPlaylistQuery query = request.ToQuery(string.Empty);

        // Assert
        Assert.Equal(string.Empty, query.PlaylistId);
    }
}
