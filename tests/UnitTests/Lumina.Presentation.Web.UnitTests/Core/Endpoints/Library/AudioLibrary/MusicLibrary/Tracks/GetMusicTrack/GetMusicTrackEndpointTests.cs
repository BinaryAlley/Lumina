#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;
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

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;

/// <summary>
/// Contains unit tests for the <see cref="GetMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicTrackEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly GetMusicTrackEndpoint _sut;
    private readonly GetMusicTrackViewRequestFixture _getMusicTrackViewRequestFixture = new();
    private readonly MusicTrackDtoFixture _musicTrackDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicTrackEndpointTests"/> class.
    /// </summary>
    public GetMusicTrackEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<GetMusicTrackEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldRequestTrackFromApiWithNormalizedIds()
    {
        // Arrange
        GetMusicTrackViewRequest request = _getMusicTrackViewRequestFixture.Create();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create();
        _mockApiHttpClient.GetAsync<MusicTrackDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedTrack);

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicTrackDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Parse(request.ArtistId), Guid.Parse(request.AlbumId), Guid.Parse(request.TrackId))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        GetMusicTrackViewRequest request = _getMusicTrackViewRequestFixture.Create(artistId: "not-a-guid", albumId: "' OR '1'='1", trackId: "'; DROP TABLE Tracks; --");
        _mockApiHttpClient.GetAsync<MusicTrackDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_musicTrackDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicTrackDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Empty, Guid.Empty, Guid.Empty)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreEmpty_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        GetMusicTrackViewRequest request = _getMusicTrackViewRequestFixture.Create(artistId: string.Empty, albumId: string.Empty, trackId: string.Empty);
        _mockApiHttpClient.GetAsync<MusicTrackDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_musicTrackDtoFixture.Create());

        // Act
        await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).GetAsync<MusicTrackDto>(
            Arg.Is<string>(endpoint => endpoint == BuildExpectedEndpoint(request.LibraryId, Guid.Empty, Guid.Empty, Guid.Empty)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsTrack_ShouldReturnSuccessJsonWithTrack()
    {
        // Arrange
        GetMusicTrackViewRequest request = _getMusicTrackViewRequestFixture.Create();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create();
        _mockApiHttpClient.GetAsync<MusicTrackDto>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expectedTrack);

        // Act
        IResult result = await _sut.ExecuteAsync(request, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedTrack.Id, jsonDocument.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    /// <summary>
    /// Builds the upstream endpoint the track retrieval is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <param name="trackId">The normalized Id of the track.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        return ApiRoutes.Music.GET_TRACK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString());
    }
}
