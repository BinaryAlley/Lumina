#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;

/// <summary>
/// Contains security tests for the <see cref="StreamMusicTrackSegmentEndpoint"/> class.
/// </summary>
/// <remarks>
/// The concurrency rate limiter of the segment route is not exercised, because it requires four concurrent long lived streams, which needs a real
/// normalization pipeline and ffmpeg, neither of which is available in the test environment.
/// </remarks>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackSegmentEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackSegmentEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StreamMusicTrackSegmentEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StreamMusicTrackSegment_WhenNotAuthenticated_ShouldRedirectToLoginWithoutCallingTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync(BuildBrowserPath(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString("N"), 0));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection in the delivery handle
    [InlineData("' OR '1'='1")] // boolean based injection in the delivery handle
    [InlineData("../../etc/passwd")] // path traversal in the delivery handle
    public async Task StreamMusicTrackSegment_WithAnInjectionPayloadInDeliveryId_ShouldRejectTheRequestWithoutCallingTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildBrowserPath(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Uri.EscapeDataString(payload),
            0));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(string.Empty, content);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.Empty(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
    }

    [Theory]
    [InlineData("artistId")] // injection in the artist Id
    [InlineData("albumId")] // injection in the album Id
    [InlineData("trackId")] // injection in the track Id
    public async Task StreamMusicTrackSegment_WithAnInjectionPayloadInATrackRouteId_ShouldNormalizeTheIdWithoutLeakingData(string routeValue)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        const string PAYLOAD = "'; DROP TABLE Tracks; --";
        Guid libraryId = Guid.NewGuid();
        string validArtistId = Guid.NewGuid().ToString();
        string validAlbumId = Guid.NewGuid().ToString();
        string validTrackId = Guid.NewGuid().ToString();
        string deliveryId = Guid.NewGuid().ToString("N");
        string artistId = routeValue == "artistId" ? Uri.EscapeDataString(PAYLOAD) : validArtistId;
        string albumId = routeValue == "albumId" ? Uri.EscapeDataString(PAYLOAD) : validAlbumId;
        string trackId = routeValue == "trackId" ? Uri.EscapeDataString(PAYLOAD) : validTrackId;
        string expectedEndpoint = ApiRoutes.Music.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", routeValue == "artistId" ? Guid.Empty.ToString() : validArtistId)
            .Replace("{albumId}", routeValue == "albumId" ? Guid.Empty.ToString() : validAlbumId)
            .Replace("{trackId}", routeValue == "trackId" ? Guid.Empty.ToString() : validTrackId)
            .Replace("{deliveryId}", deliveryId)
            .Replace("{segmentIndex}", "0");
        byte[] contentBytes = CreateBytes(128);
        _apiFactory.StreamingApiClientStub.RegisterStreamResponse(expectedEndpoint, await CreateStreamResultAsync(contentBytes));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(
            $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track-segment/{deliveryId}/0?libraryId={libraryId}");
        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(contentBytes, responseBytes);
        Assert.DoesNotContain("SQL", System.Text.Encoding.UTF8.GetString(responseBytes), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", System.Text.Encoding.UTF8.GetString(responseBytes), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", System.Text.Encoding.UTF8.GetString(responseBytes), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(PAYLOAD, System.Text.Encoding.UTF8.GetString(responseBytes), StringComparison.Ordinal);
        Assert.Single(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
        Assert.Equal(expectedEndpoint, _apiFactory.StreamingApiClientStub.GetEndpointsCalled[0]);
    }

    [Fact]
    public async Task StreamMusicTrackSegment_WhenTheApiDeclaresAPublicCache_ShouldNeverExposeAPublicCacheControl()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string deliveryId = Guid.NewGuid().ToString("N");
        string expectedEndpoint = ApiRoutes.Music.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString())
            .Replace("{deliveryId}", deliveryId)
            .Replace("{segmentIndex}", "0");
        _apiFactory.StreamingApiClientStub.RegisterStreamResponse(expectedEndpoint, await CreateStreamResultAsync(CreateBytes(64)));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildBrowserPath(libraryId, artistId, albumId, trackId, deliveryId, 0));

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        string cacheControl = response.Headers.CacheControl?.ToString() ?? string.Empty;
        Assert.DoesNotContain("public", cacheControl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("private", cacheControl);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    /// <summary>
    /// Builds the browser facing route of a single segment of the normalized audio of a track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <param name="deliveryId">The opaque handle of the normalized delivery.</param>
    /// <param name="segmentIndex">The zero based index of the segment.</param>
    /// <returns>The browser facing route of the requested segment.</returns>
    private static string BuildBrowserPath(Guid libraryId, Guid artistId, Guid albumId, Guid trackId, string deliveryId, int segmentIndex)
    {
        return $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track-segment/{deliveryId}/{segmentIndex}?libraryId={libraryId}";
    }

    /// <summary>
    /// Creates a streamed API response with a partial content status and a body.
    /// </summary>
    /// <param name="contentBytes">The bytes of the content of the response.</param>
    /// <returns>The created streamed response.</returns>
    private static async Task<ApiStreamResult> CreateStreamResultAsync(byte[] contentBytes)
    {
        HttpResponseMessage response = new(HttpStatusCode.PartialContent)
        {
            Content = new ByteArrayContent(contentBytes)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp4");
        response.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(120) };
        return await ApiStreamResult.CreateAsync(response, CancellationToken.None);
    }

    /// <summary>
    /// Creates a deterministic byte array that stands in for the content of a segment.
    /// </summary>
    /// <param name="length">The number of bytes to create.</param>
    /// <returns>The created bytes.</returns>
    private static byte[] CreateBytes(int length)
    {
        return [.. Enumerable.Range(0, length).Select(index => (byte)(index % 256))];
    }
}
