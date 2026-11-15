#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreMusicTrack;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.UnitTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreMusicTrack;

/// <summary>
/// Contains unit tests for the <see cref="RestoreMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreMusicTrackEndpointTests
{
    private readonly IApiHttpClient _mockApiHttpClient;
    private readonly RestoreMusicTrackEndpoint _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreMusicTrackEndpointTests"/> class.
    /// </summary>
    public RestoreMusicTrackEndpointTests()
    {
        _mockApiHttpClient = Substitute.For<IApiHttpClient>();
        // The restore endpoints forward a bodyless POST upstream; returning an empty request lets every test that is not about
        // cancellation focus on the URL construction instead of repeating the upstream stub.
        _mockApiHttpClient.PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            Arg.Any<string>(), Arg.Any<Web.Common.Requests.Common.EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(new Web.Common.Requests.Common.EmptyRequest());
        _sut = Factory.Create<RestoreMusicTrackEndpoint>(_mockApiHttpClient);
    }

    [Theory]
    [InlineData("D")] // Standard hyphenated format.
    [InlineData("B")] // Braced format.
    [InlineData("N")] // Compact format.
    [InlineData("P")] // Parenthesized format.
    public async Task ExecuteAsync_WhenCalled_ShouldRestoreTrackViaApiWithNormalizedIdsAndReturnSuccessPayload(string guidFormat)
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(guidFormat), albumId.ToString(guidFormat), trackId.ToString(guidFormat), libraryId.ToString(guidFormat));

        // Act
        IResult result = await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);
        string body = await JsonResultTestHelper.GetResponseBodyAsync(result);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, artistId, albumId, trackId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
        using JsonDocument jsonDocument = JsonDocument.Parse(body);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        Assert.True(jsonDocument.RootElement.GetProperty("data").GetProperty("isRestored").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsNotParseable_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest("not-a-guid", albumId.ToString(), trackId.ToString(), libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, Guid.Empty, albumId, trackId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenArtistRouteIdIsMissing_ShouldForwardGuidEmptyArtistIdToApi()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(null, albumId.ToString(), trackId.ToString(), libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, Guid.Empty, albumId, trackId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlbumRouteIdIsNotParseable_ShouldForwardGuidEmptyAlbumIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), "not-a-guid", trackId.ToString(), libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, artistId, Guid.Empty, trackId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlbumRouteIdIsMissing_ShouldForwardGuidEmptyAlbumIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), null, trackId.ToString(), libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, artistId, Guid.Empty, trackId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackRouteIdIsNotParseable_ShouldForwardGuidEmptyTrackIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), "not-a-guid", libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, artistId, albumId, Guid.Empty),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTrackRouteIdIsMissing_ShouldForwardGuidEmptyTrackIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), null, libraryId.ToString());

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(libraryId, artistId, albumId, Guid.Empty),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
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
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(Guid.Empty, artistId, albumId, trackId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLibraryQueryIdIsMissing_ShouldForwardGuidEmptyLibraryIdToApi()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), null);

        // Act
        await _sut.ExecuteAsync(EmptyRequest.Instance, CancellationToken.None);

        // Assert
        await _mockApiHttpClient.Received(1).PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            BuildExpectedEndpoint(Guid.Empty, artistId, albumId, trackId),
            Arg.Any<Web.Common.Requests.Common.EmptyRequest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldCancelOperation()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        ConfigureRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), libraryId.ToString());
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        _mockApiHttpClient.PostAsync<Web.Common.Requests.Common.EmptyRequest, Web.Common.Requests.Common.EmptyRequest>(
            Arg.Any<string>(), Arg.Any<Web.Common.Requests.Common.EmptyRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<Web.Common.Requests.Common.EmptyRequest>(cancellationTokenSource.Token));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.ExecuteAsync(EmptyRequest.Instance, cancellationTokenSource.Token));
    }

    /// <summary>
    /// Sets the route and query values the endpoint reads the track identity from.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id, or <see langword="null"/> to leave the route value missing.</param>
    /// <param name="albumId">The raw route value of the album Id, or <see langword="null"/> to leave the route value missing.</param>
    /// <param name="trackId">The raw route value of the track Id, or <see langword="null"/> to leave the route value missing.</param>
    /// <param name="libraryId">The raw query value of the library Id, or <see langword="null"/> to leave the query string missing.</param>
    private void ConfigureRequest(string? artistId, string? albumId, string? trackId, string? libraryId)
    {
        if (artistId is not null)
            _sut.HttpContext.Request.RouteValues["artistId"] = artistId;
        if (albumId is not null)
            _sut.HttpContext.Request.RouteValues["albumId"] = albumId;
        if (trackId is not null)
            _sut.HttpContext.Request.RouteValues["trackId"] = trackId;
        if (libraryId is not null)
            _sut.HttpContext.Request.QueryString = new QueryString($"?libraryId={libraryId}");
    }

    /// <summary>
    /// Builds the upstream endpoint the track restoration is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The normalized Id of the media library.</param>
    /// <param name="artistId">The normalized Id of the artist.</param>
    /// <param name="albumId">The normalized Id of the album.</param>
    /// <param name="trackId">The normalized Id of the track.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        return ApiRoutes.Music.RESTORE_TRACK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString());
    }
}
