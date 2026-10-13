#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.SaveMusicTrack;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.SaveMusicTrack;

/// <summary>
/// Contains unit tests for the <see cref="SaveMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicTrackEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly SaveMusicTrackEndpoint _sut;
    private readonly UpdateMusicTrackRequestFixture _updateMusicTrackRequestFixture = new();
    private readonly MusicTrackDtoFixture _musicTrackDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicTrackEndpointTests"/> class.
    /// </summary>
    public SaveMusicTrackEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<SaveMusicTrackEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldForwardUpdateMusicTrackRequestToApiWithRouteAndQueryIds()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicTrackRequest request = _updateMusicTrackRequestFixture.Create();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create();
        _mockApiHttpClient.PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicTrackRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedTrack);
        ConfigureRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, artistId, albumId, trackId)),
            Arg.Is<UpdateMusicTrackRequest>(forwardedRequest => forwardedRequest == request),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        const string MALICIOUS_ARTIST_ID = "not-a-guid";
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicTrackRequest request = _updateMusicTrackRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicTrackRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicTrackDtoFixture.Create());
        ConfigureRequest(MALICIOUS_ARTIST_ID, albumId.ToString(), trackId.ToString(), libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, Guid.Empty, albumId, trackId)),
            Arg.Any<UpdateMusicTrackRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlbumRouteIdIsNotParseable_ShouldForwardGuidEmptyAlbumIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        const string MALICIOUS_ALBUM_ID = "' OR '1'='1";
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicTrackRequest request = _updateMusicTrackRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicTrackRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicTrackDtoFixture.Create());
        ConfigureRequest(artistId.ToString(), MALICIOUS_ALBUM_ID, trackId.ToString(), libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, artistId, Guid.Empty, trackId)),
            Arg.Any<UpdateMusicTrackRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackRouteIdIsNotParseable_ShouldForwardGuidEmptyTrackIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        const string MALICIOUS_TRACK_ID = "'; DROP TABLE Tracks; --";
        Guid libraryId = Guid.NewGuid();
        UpdateMusicTrackRequest request = _updateMusicTrackRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicTrackRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicTrackDtoFixture.Create());
        ConfigureRequest(artistId.ToString(), albumId.ToString(), MALICIOUS_TRACK_ID, libraryId);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, artistId, albumId, Guid.Empty)),
            Arg.Any<UpdateMusicTrackRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreMissing_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        UpdateMusicTrackRequest request = _updateMusicTrackRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicTrackRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicTrackDtoFixture.Create());
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(libraryId, Guid.Empty, Guid.Empty, Guid.Empty)),
            Arg.Any<UpdateMusicTrackRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        const string MALICIOUS_LIBRARY_ID = "not-a-guid";
        UpdateMusicTrackRequest request = _updateMusicTrackRequestFixture.Create();
        _mockApiHttpClient.PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicTrackRequest>(), Arg.Any<CancellationToken>())
            .Returns(_musicTrackDtoFixture.Create());
        ConfigureRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), MALICIOUS_LIBRARY_ID);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(Guid.Empty, artistId, albumId, trackId)),
            Arg.Any<UpdateMusicTrackRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsUpdatedTrack_ShouldReturnSuccessJsonWithTrack()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateMusicTrackRequest request = _updateMusicTrackRequestFixture.Create();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create();
        _mockApiHttpClient.PutAsync<MusicTrackDto, UpdateMusicTrackRequest>(Arg.Any<string>(), Arg.Any<UpdateMusicTrackRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedTrack);
        ConfigureRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), libraryId);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedTrack.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the track identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="trackId">The raw route value of the track Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, string albumId, string trackId, Guid libraryId)
    {
        ConfigureRequest(artistId, albumId, trackId, libraryId.ToString());
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the track identities from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="trackId">The raw route value of the track Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    private void ConfigureRequest(string artistId, string albumId, string trackId, string libraryId)
    {
        _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        _sut.HttpContext.Request.RouteValues["trackId"] = trackId;
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");
    }

    /// <summary>
    /// Builds the upstream endpoint the track update is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <param name="trackId">The normalized Id of the track.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        return ApiRoutes.Music.UPDATE_TRACK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString());
    }
}
