#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Fixtures.Core.Requests.Playlists;
using Lumina.Contracts.Requests.Playlists;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Contracts.UnitTests.Requests.Playlists;

/// <summary>
/// Contains unit tests for the <see cref="GetPlaylistRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistRequestTests
{
    private readonly GetPlaylistRequestFixture _getPlaylistRequestFixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistRequestTests"/> class.
    /// </summary>
    public GetPlaylistRequestTests()
    {
        _getPlaylistRequestFixture = new GetPlaylistRequestFixture();
    }

    [Fact]
    public void Constructor_WhenCreatedWithFixtureDefaults_ShouldProduceAValidRecord()
    {
        // Act
        GetPlaylistRequest sut = _getPlaylistRequestFixture.Create();

        // Assert
        Assert.NotNull(sut.CurrentPage);
        Assert.True(sut.CurrentPage.Value >= 1);
        Assert.NotNull(sut.PerPage);
        Assert.True(sut.PerPage.Value >= 1);
    }

    [Fact]
    public void Constructor_WhenCurrentPageIsSupplied_ShouldPreserveItIndependently()
    {
        // Act
        GetPlaylistRequest sut = _getPlaylistRequestFixture.Create(currentPage: 8, perPage: 40);

        // Assert
        Assert.NotNull(sut.CurrentPage);
        Assert.Equal(8, sut.CurrentPage);
        Assert.NotNull(sut.PerPage);
        Assert.Equal(40, sut.PerPage);
    }

    [Fact]
    public void Constructor_WhenPerPageIsSupplied_ShouldPreserveItIndependently()
    {
        // Act
        GetPlaylistRequest sut = _getPlaylistRequestFixture.Create(currentPage: 2, perPage: 75);

        // Assert
        Assert.NotNull(sut.PerPage);
        Assert.Equal(75, sut.PerPage);
        Assert.NotNull(sut.CurrentPage);
        Assert.Equal(2, sut.CurrentPage);
    }

    [Fact]
    public void Constructor_WhenCurrentPageIsNull_ShouldPreserveNullWhileKeepingThePerPage()
    {
        // Act
        GetPlaylistRequest sut = _getPlaylistRequestFixture.Create(includeCurrentPage: false);

        // Assert
        Assert.Null(sut.CurrentPage);
        Assert.NotNull(sut.PerPage);
    }

    [Fact]
    public void Constructor_WhenPerPageIsNull_ShouldPreserveNullWhileKeepingTheCurrentPage()
    {
        // Act
        GetPlaylistRequest sut = _getPlaylistRequestFixture.Create(includePerPage: false);

        // Assert
        Assert.NotNull(sut.CurrentPage);
        Assert.Null(sut.PerPage);
    }
}
