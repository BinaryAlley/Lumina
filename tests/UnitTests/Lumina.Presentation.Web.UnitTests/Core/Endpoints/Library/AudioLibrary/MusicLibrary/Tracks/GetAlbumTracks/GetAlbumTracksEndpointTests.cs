#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetAlbumTracks;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetAlbumTracks;

/// <summary>
/// Contains unit tests for the <see cref="GetAlbumTracksEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetAlbumTracksEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetAlbumTracksEndpoint _sut;
    private readonly GetAlbumTracksLiteRequestFixture _getAlbumTracksLiteRequestFixture = new();
    private readonly PaginatedMusicTrackLiteDtoFixture _paginatedMusicTrackLiteDtoFixture = new();
    private readonly MusicTrackLiteDtoFixture _musicTrackLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAlbumTracksEndpointTests"/> class.
    /// </summary>
    public GetAlbumTracksEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetAlbumTracksEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithPagination_ShouldRequestTracksWithQueryParameters()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(currentPage: 2, perPage: 48);
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicTrackLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicTrackLiteDto>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId), Guid.Parse(request.AlbumId))}?currentPage=2&perPage=48"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithoutPagination_ShouldRequestTracksWithoutQueryParameters()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create();
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicTrackLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicTrackLiteDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId), Guid.Parse(request.AlbumId))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyCurrentPageIsProvided_ShouldAppendOnlyCurrentPage()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(currentPage: 3);
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicTrackLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicTrackLiteDto>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId), Guid.Parse(request.AlbumId))}?currentPage=3"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyPerPageIsProvided_ShouldAppendOnlyPerPage()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(perPage: 24);
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicTrackLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicTrackLiteDto>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId), Guid.Parse(request.AlbumId))}?perPage=24"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(artistId: "not-a-guid", currentPage: 1, perPage: 20);
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicTrackLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicTrackLiteDto>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Empty, Guid.Parse(request.AlbumId))}?currentPage=1&perPage=20"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlbumIdIsNotParseable_ShouldForwardGuidEmptyAlbumIdToApi()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(albumId: "' OR '1'='1", currentPage: 1, perPage: 20);
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicTrackLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicTrackLiteDto>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId), Guid.Empty)}?currentPage=1&perPage=20"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistIdAndAlbumIdAreMissing_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create(artistId: string.Empty, albumId: string.Empty, currentPage: 1, perPage: 20);
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicTrackLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedMusicTrackLiteDto>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Empty, Guid.Empty)}?currentPage=1&perPage=20"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsPaginatedTracks_ShouldReturnSuccessJsonWithTracks()
    {
        // Arrange
        GetAlbumTracksLiteRequest request = _getAlbumTracksLiteRequestFixture.Create();
        PaginatedMusicTrackLiteDto expectedResponse = _paginatedMusicTrackLiteDtoFixture.Create(
            data: [_musicTrackLiteDtoFixture.Create(title: "Test Track")],
            currentPage: 1,
            perPage: 20,
            count: 1,
            numberOfPages: 1);
        _mockApiHttpClient.GetAsync<PaginatedMusicTrackLiteDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(1, data.GetProperty("data").GetArrayLength());
    }

    /// <summary>
    /// Builds the upstream endpoint the album track retrieval is expected to be sent to, without query parameters.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId)
    {
        return ApiRoutes.Music.GET_ALBUM_TRACKS_LITE
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString());
    }
}
