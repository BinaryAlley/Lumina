#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetMusicAlbum;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetMusicAlbum;

/// <summary>
/// Contains unit tests for the <see cref="GetMusicAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicAlbumEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetMusicAlbumEndpoint _sut;
    private readonly GetMusicAlbumViewRequestFixture _getMusicAlbumViewRequestFixture = new();
    private readonly MusicAlbumDtoFixture _musicAlbumDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicAlbumEndpointTests"/> class.
    /// </summary>
    public GetMusicAlbumEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetMusicAlbumEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestAlbumFromApiWithNormalizedIds()
    {
        // Arrange
        GetMusicAlbumViewRequest request = _getMusicAlbumViewRequestFixture.Create();
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create();
        _mockApiHttpClient.GetAsync<MusicAlbumDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedAlbum);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicAlbumDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId), Guid.Parse(request.AlbumId))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        GetMusicAlbumViewRequest request = _getMusicAlbumViewRequestFixture.Create(artistId: "not-a-guid", albumId: "' OR '1'='1");
        _mockApiHttpClient.GetAsync<MusicAlbumDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_musicAlbumDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicAlbumDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Empty, Guid.Empty)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreEmpty_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        GetMusicAlbumViewRequest request = _getMusicAlbumViewRequestFixture.Create(artistId: string.Empty, albumId: string.Empty);
        _mockApiHttpClient.GetAsync<MusicAlbumDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_musicAlbumDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicAlbumDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Empty, Guid.Empty)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsAlbum_ShouldReturnSuccessJsonWithAlbum()
    {
        // Arrange
        GetMusicAlbumViewRequest request = _getMusicAlbumViewRequestFixture.Create();
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create();
        _mockApiHttpClient.GetAsync<MusicAlbumDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedAlbum);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedAlbum.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    /// <summary>
    /// Builds the upstream endpoint the album retrieval is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId)
    {
        return ApiRoutes.Music.GET_ALBUM
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString());
    }
}
