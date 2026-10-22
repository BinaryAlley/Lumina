#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetArtistAlbums;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetArtistAlbums;

/// <summary>
/// Contains unit tests for the <see cref="GetArtistAlbumsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetArtistAlbumsEndpoint _sut;
    private readonly GetArtistAlbumsLiteRequestFixture _getArtistAlbumsLiteRequestFixture = new();
    private readonly PaginatedDtoFixture<MusicAlbumLiteDto> _paginatedMusicAlbumLiteDtoFixture = new();
    private readonly MusicAlbumLiteDtoFixture _musicAlbumLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsEndpointTests"/> class.
    /// </summary>
    public GetArtistAlbumsEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetArtistAlbumsEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithPagination_ShouldRequestAlbumsWithQueryParameters()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(currentPage: 2, perPage: 48);
        _mockApiHttpClient.GetAsync<PaginatedDto<MusicAlbumLiteDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicAlbumLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<MusicAlbumLiteDto>>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId))}?currentPage=2&perPage=48"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalledWithoutPagination_ShouldRequestAlbumsWithoutQueryParameters()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        _mockApiHttpClient.GetAsync<PaginatedDto<MusicAlbumLiteDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicAlbumLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<MusicAlbumLiteDto>>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyCurrentPageIsProvided_ShouldAppendOnlyCurrentPage()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(currentPage: 3);
        _mockApiHttpClient.GetAsync<PaginatedDto<MusicAlbumLiteDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicAlbumLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<MusicAlbumLiteDto>>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId))}?currentPage=3"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyPerPageIsProvided_ShouldAppendOnlyPerPage()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(perPage: 24);
        _mockApiHttpClient.GetAsync<PaginatedDto<MusicAlbumLiteDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicAlbumLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<MusicAlbumLiteDto>>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId))}?perPage=24"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create(artistId: "not-a-guid", currentPage: 1, perPage: 20);
        _mockApiHttpClient.GetAsync<PaginatedDto<MusicAlbumLiteDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_paginatedMusicAlbumLiteDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<PaginatedDto<MusicAlbumLiteDto>>(
            Arg.Is<string>(endpoint => endpoint == $"{BuildExpectedEndpoint(request.LibraryId, Guid.Empty)}?currentPage=1&perPage=20"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsPaginatedAlbums_ShouldReturnSuccessJsonWithAlbums()
    {
        // Arrange
        GetArtistAlbumsLiteRequest request = _getArtistAlbumsLiteRequestFixture.Create();
        PaginatedDto<MusicAlbumLiteDto> expectedResponse = _paginatedMusicAlbumLiteDtoFixture.Create(
            data: [_musicAlbumLiteDtoFixture.Create(title: "Test Album")],
            currentPage: 1,
            perPage: 20,
            count: 1,
            numberOfPages: 1);
        _mockApiHttpClient.GetAsync<PaginatedDto<MusicAlbumLiteDto>>(Arg.Any<string>(), Arg.Any<CancellationToken>())
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
    /// Builds the upstream endpoint the artist album retrieval is expected to be sent to, without query parameters.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId)
    {
        return ApiRoutes.Music.GET_ARTIST_ALBUMS_LITE
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString());
    }
}
