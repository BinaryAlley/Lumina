#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;

/// <summary>
/// Contains unit tests for the <see cref="SaveMusicAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicAlbumEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SaveMusicAlbumEndpoint _sut;
    private readonly UpdateMusicAlbumRequestFixture _updateMusicAlbumRequestFixture = new();
    private readonly MusicAlbumDtoFixture _musicAlbumDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicAlbumEndpointTests"/> class.
    /// </summary>
    public SaveMusicAlbumEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<SaveMusicAlbumEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardUpdateMusicAlbumRequestToApiWithRouteAndQueryIds()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create();
        _mockApiHttpClient.PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicAlbumRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedAlbum);
        ConfigureRequest(artistId.ToString(), albumId.ToString(), libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, artistId, albumId)),
            Arg.Is<UpdateMusicAlbumRequest>(forwardedRequest => forwardedRequest == request),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        const string MALICIOUS_ARTIST_ID = "not-a-guid";
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicAlbumRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicAlbumDtoFixture.Create());
        ConfigureRequest(MALICIOUS_ARTIST_ID, albumId.ToString(), libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, Guid.Empty, albumId)),
            Arg.Any<UpdateMusicAlbumRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlbumRouteIdIsNotParseable_ShouldForwardGuidEmptyAlbumIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        const string MALICIOUS_ALBUM_ID = "' OR '1'='1";
        Guid libraryId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicAlbumRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicAlbumDtoFixture.Create());
        ConfigureRequest(artistId.ToString(), MALICIOUS_ALBUM_ID, libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, artistId, Guid.Empty)),
            Arg.Any<UpdateMusicAlbumRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreMissing_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicAlbumRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicAlbumDtoFixture.Create());
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, Guid.Empty, Guid.Empty)),
            Arg.Any<UpdateMusicAlbumRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        const string MALICIOUS_LIBRARY_ID = "not-a-guid";
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicAlbumRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicAlbumDtoFixture.Create());
        ConfigureRequest(artistId.ToString(), albumId.ToString(), MALICIOUS_LIBRARY_ID);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(Guid.Empty, artistId, albumId)),
            Arg.Any<UpdateMusicAlbumRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsUpdatedAlbum_ShouldReturnSuccessJsonWithAlbum()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create();
        _mockApiHttpClient.PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicAlbumRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedAlbum);
        ConfigureRequest(artistId.ToString(), albumId.ToString(), libraryId);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedAlbum.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the album identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, string albumId, Guid libraryId)
    {
        ConfigureRequest(artistId, albumId, libraryId.ToString());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the album identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, string albumId, string libraryId)
    {
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");
    }

    /// <summary>
    /// Builds the upstream endpoint the album update is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId)
    {
        return ApiRoutes.Music.UPDATE_ALBUM
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString());
    }
}
