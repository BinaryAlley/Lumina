#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;

/// <summary>
/// Contains security tests for the <see cref="StreamMusicTrackEndpoint"/> class.
/// </summary>
/// <remarks>
/// The concurrency rate limiter of the stream route is not exercised, because it requires four concurrent long lived streams, which needs a real
/// normalization pipeline and ffmpeg, neither of which is available in the test environment.
/// </remarks>
[ExcludeFromCodeCoverage]
public class StreamMusicTrackEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StreamMusicTrackEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StreamMusicTrack_WhenNotAuthenticated_ShouldRedirectToLoginWithoutCallingTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync(BuildBrowserPath(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
    }

    [Theory]
    [InlineData("artistId")] // injection in the artist Id
    [InlineData("albumId")] // injection in the album Id
    [InlineData("trackId")] // injection in the track Id
    public async Task StreamMusicTrack_WithAnInjectionPayloadInATrackRouteId_ShouldNormalizeTheIdWithoutLeakingData(string routeValue)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        const string PAYLOAD = "'; DROP TABLE Tracks; --";
        Guid libraryId = Guid.NewGuid();
        string validArtistId = Guid.NewGuid().ToString();
        string validAlbumId = Guid.NewGuid().ToString();
        string validTrackId = Guid.NewGuid().ToString();
        string artistId = routeValue == "artistId" ? Uri.EscapeDataString(PAYLOAD) : validArtistId;
        string albumId = routeValue == "albumId" ? Uri.EscapeDataString(PAYLOAD) : validAlbumId;
        string trackId = routeValue == "trackId" ? Uri.EscapeDataString(PAYLOAD) : validTrackId;
        string expectedEndpoint = ApiRoutes.Music.STREAM_TRACK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", routeValue == "artistId" ? Guid.Empty.ToString() : validArtistId)
            .Replace("{albumId}", routeValue == "albumId" ? Guid.Empty.ToString() : validAlbumId)
            .Replace("{trackId}", routeValue == "trackId" ? Guid.Empty.ToString() : validTrackId);
        byte[] contentBytes = CreateBytes(128);
        _apiFactory.StreamingApiClientStub.RegisterStreamResponse(expectedEndpoint, await CreateStreamResultAsync(contentBytes));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(
            $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track?libraryId={libraryId}");
        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
        string responseText = Encoding.UTF8.GetString(responseBytes);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(contentBytes, responseBytes);
        Assert.DoesNotContain("SQL", responseText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", responseText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", responseText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(PAYLOAD, responseText, StringComparison.Ordinal);
        Assert.Single(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
        Assert.Equal(expectedEndpoint, _apiFactory.StreamingApiClientStub.GetEndpointsCalled[0]);
    }

    [Fact]
    public async Task StreamMusicTrack_WhenTheApiDeclaresAPublicCache_ShouldNeverExposeAPublicCacheControl()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string expectedEndpoint = ApiRoutes.Music.STREAM_TRACK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString());
        _apiFactory.StreamingApiClientStub.RegisterStreamResponse(expectedEndpoint, await CreateStreamResultAsync(CreateBytes(64)));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildBrowserPath(libraryId, artistId, albumId, trackId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string cacheControl = response.Headers.CacheControl?.ToString() ?? string.Empty;
        Assert.DoesNotContain("public", cacheControl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("private", cacheControl);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    /// <summary>
    /// Builds the browser facing route of the direct stream of a track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The browser facing route of the requested stream.</returns>
    private static string BuildBrowserPath(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        return $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track?libraryId={libraryId}";
    }

    /// <summary>
    /// Creates a streamed API response with an ok status and a body.
    /// </summary>
    /// <param name="contentBytes">The bytes of the content of the response.</param>
    /// <returns>The created streamed response.</returns>
    private static async Task<ApiStreamResult> CreateStreamResultAsync(byte[] contentBytes)
    {
        HttpResponseMessage response = new(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(contentBytes)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");
        response.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(120) };
        return await ApiStreamResult.CreateAsync(response, CancellationToken.None);
    }

    /// <summary>
    /// Creates a deterministic byte array that stands in for the content of a stream.
    /// </summary>
    /// <param name="length">The number of bytes to create.</param>
    /// <returns>The created bytes.</returns>
    private static byte[] CreateBytes(int length)
    {
        return [.. Enumerable.Range(0, length).Select(index => (byte)(index % 256))];
    }
}
