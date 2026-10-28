#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaybackManifestEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string VALID_DELIVERY_ID = "0123456789abcdef0123456789abcdef";
    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaybackManifestDtoFixture _playbackManifestDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPlaybackManifestEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenManifestIsNotADirectPlay_ShouldReturnTheWebProxyPaths()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        PlaybackManifestDto manifest = _playbackManifestDtoFixture.Create(
            trackId: trackId,
            isDirectPlay: false,
            deliveryId: VALID_DELIVERY_ID,
            segmentStreamPathTemplate: "/api/v1/libraries/ancient/stream/deliveries/{deliveryId}/segments/{segmentIndex}");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest", manifest);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(libraryId, artistId, albumId, trackId);
        string expectedStreamPath = $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track?libraryId={libraryId}";
        string expectedSegmentStreamPathTemplate = $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track-segment/{{deliveryId}}/{{segmentIndex}}?libraryId={libraryId}";

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedStreamPath, data.GetProperty("streamPath").GetString());
        Assert.Equal(expectedSegmentStreamPathTemplate, data.GetProperty("segmentStreamPathTemplate").GetString());
        Assert.Equal(VALID_DELIVERY_ID, data.GetProperty("deliveryId").GetString());
        Assert.DoesNotContain("http", data.GetProperty("streamPath").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http", data.GetProperty("segmentStreamPathTemplate").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenManifestIsADirectPlay_ShouldReturnNullDeliveryAndTemplate()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        PlaybackManifestDto manifest = _playbackManifestDtoFixture.Create(
            trackId: trackId,
            isDirectPlay: true,
            includeDeliveryId: false,
            includeSegmentStreamPathTemplate: false);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest", manifest);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(libraryId, artistId, albumId, trackId);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Null, data.GetProperty("deliveryId").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("segmentStreamPathTemplate").ValueKind);
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenApiDeclaresATemplateWithAnInvalidDeliveryId_ShouldReturnBadGateway()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        PlaybackManifestDto manifest = _playbackManifestDtoFixture.Create(
            trackId: trackId,
            isDirectPlay: false,
            deliveryId: "not-a-valid-handle",
            segmentStreamPathTemplate: "/api/v1/libraries/ancient/stream/deliveries/{deliveryId}/segments/{segmentIndex}");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest", manifest);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(libraryId, artistId, albumId, trackId);
        // The player fetch classifies as a raw media request, which is answered with a bare status instead of a page or a JSON body.
        getRequest.Headers.Accept.Clear();
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(string.Empty, content);
    }

    [Fact]
    public async Task GetPlaybackManifest_WhenNotAuthenticated_ShouldRedirectToLoginWithoutCallingTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/api-get-playback-manifest?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task GetPlaybackManifest_WithSQLInjectionInRouteId_ShouldNormalizeTheIdWithoutLeakingData(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        PlaybackManifestDto manifest = _playbackManifestDtoFixture.Create(isDirectPlay: true, includeDeliveryId: false, includeSegmentStreamPathTemplate: false);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}/playback-manifest", manifest);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(maliciousId)}/albums/{Uri.EscapeDataString(maliciousId)}/tracks/{Uri.EscapeDataString(maliciousId)}/api-get-playback-manifest?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (JsonDocument json = JsonDocument.Parse(content))
            Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousId, content, StringComparison.Ordinal);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == $"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}/playback-manifest");
    }

    /// <summary>
    /// Creates an authenticated JSON request for the playback manifest of a track.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The configured request.</returns>
    private static HttpRequestMessage CreateGetRequest(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-playback-manifest?libraryId={libraryId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return getRequest;
    }
}
