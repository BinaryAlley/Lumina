#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;

/// <summary>
/// Contains unit tests for the <see cref="DeleteMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicTrackEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly DeleteMusicTrackEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicTrackEndpointTests"/> class.
    /// </summary>
    public DeleteMusicTrackEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        _sut = Factory.Create<DeleteMusicTrackEndpoint>(_mockApiHttpClient);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCalled_ShouldDeleteTrackViaApiAndReturnSuccessWithoutData()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), libraryId);

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(libraryId, artistId, albumId, trackId),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.False(jsonDocument.RootElement.TryGetProperty("data", out JsonElement _));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreNotParseable_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest("not-a-guid", "not-a-guid", "not-a-guid", libraryId);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(libraryId, Guid.Empty, Guid.Empty, Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenRouteIdsAreMissing_ShouldForwardGuidEmptyIdsToApi()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(libraryId, Guid.Empty, Guid.Empty, Guid.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsNotParseable_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), "not-a-guid");

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).DeleteAsync(
            BuildExpectedEndpoint(Guid.Empty, artistId, albumId, trackId),
            Arg.Any<CancellationToken>());
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
    /// Builds the upstream endpoint the track deletion is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <param name="trackId">The normalized Id of the track.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        return ApiRoutes.Music.DELETE_TRACK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString());
    }
}
