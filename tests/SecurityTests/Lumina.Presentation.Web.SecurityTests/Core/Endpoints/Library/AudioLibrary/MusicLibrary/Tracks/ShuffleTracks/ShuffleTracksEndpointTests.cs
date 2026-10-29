#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.ShuffleTracks;

/// <summary>
/// Contains security tests for the <see cref="ShuffleTracksEndpoint"/> class.
/// </summary>
/// <remarks>
/// The shuffle route declares no rate limiter policy, so rate limiting is not exercised here. Token and claim correctness does not apply, because the Web
/// layer issues no token for this route, and timing attack resistance does not apply either, because the route performs no credential comparison.
/// </remarks>
[ExcludeFromCodeCoverage]
public class ShuffleTracksEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string SHUFFLE_TRACKS_PATH = "/en-us/library/audio-library/music-library/tracks/api-shuffle-tracks";
    private readonly LuminaWebFactory _apiFactory;
    private readonly ShuffleTracksRequestFixture _shuffleTracksRequestFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ShuffleTracksEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ShuffleTracksEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ShuffleTracks_WhenNotAuthenticated_ShouldRedirectToLoginWithoutCallingTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create();

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(CreateShuffleRequest(request));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => IsShuffleCall(postRequest.Endpoint));
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task ShuffleTracks_WithInjectionInLibraryId_ShouldRejectTheRequestWithoutCallingTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateRawShuffleRequest(
            $"{{\"libraryId\": {JsonSerializer.Serialize(payload)}, \"count\": 10}}"));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        AssertRejectedWithoutLeak(content, payload);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task ShuffleTracks_WithInjectionInCount_ShouldRejectTheRequestWithoutCallingTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateRawShuffleRequest(
            $"{{\"libraryId\": \"{Guid.NewGuid()}\", \"count\": {JsonSerializer.Serialize(payload)}}}"));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        AssertRejectedWithoutLeak(content, payload);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task ShuffleTracks_WithInjectionInExcludedTrackIds_ShouldRejectTheRequestWithoutCallingTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateRawShuffleRequest(
            $"{{\"libraryId\": \"{Guid.NewGuid()}\", \"count\": 10, \"excludedTrackIds\": [{JsonSerializer.Serialize(payload)}]}}"));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        AssertRejectedWithoutLeak(content, payload);
    }

    [Fact]
    public async Task ShuffleTracks_WhenApiRejectsTheRequest_ShouldReturnACleanErrorWithoutLeakingData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        ShuffleTracksRequest request = _shuffleTracksRequestFixture.Create(libraryId: libraryId);
        string expectedEndpoint = ApiRoutes.Music.SHUFFLE_TRACKS.Replace("{libraryId}", libraryId.ToString());
        _apiFactory.ApiClientStub.RegisterPostException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Unauthorized", detail: "NotAuthorized"),
            HttpStatusCode.Forbidden,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(CreateShuffleRequest(request));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The Web layer answers programmatic callers with a sanitized JSON error instead of forwarding the upstream problem details verbatim.
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("not authorized", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == expectedEndpoint);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Asserts that a request rejected before reaching the API was answered with a sanitized JSON error that does not echo the payload.
    /// </summary>
    /// <param name="content">The body of the response.</param>
    /// <param name="payload">The malicious payload that must not be echoed.</param>
    private void AssertRejectedWithoutLeak(string content, string payload)
    {
        // The Web layer reports binding failures to programmatic callers as a JSON error with the success flag cleared, and never forwards the request upstream.
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => IsShuffleCall(postRequest.Endpoint));
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether the specified upstream endpoint is the shuffle endpoint.
    /// </summary>
    /// <param name="endpoint">The upstream endpoint to check.</param>
    /// <returns><see langword="true"/> when the endpoint is the shuffle endpoint, <see langword="false"/> otherwise.</returns>
    private static bool IsShuffleCall(string endpoint)
    {
        return endpoint.EndsWith("/tracks/shuffle", StringComparison.Ordinal);
    }

    /// <summary>
    /// Builds the Web shuffle request that posts the given <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The shuffle request to send.</param>
    /// <returns>The configured Web request.</returns>
    private static HttpRequestMessage CreateShuffleRequest(ShuffleTracksRequest request)
    {
        HttpRequestMessage shuffleRequest = new(HttpMethod.Post, SHUFFLE_TRACKS_PATH)
        {
            Content = JsonContent.Create(request)
        };
        shuffleRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        shuffleRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return shuffleRequest;
    }

    /// <summary>
    /// Builds the Web shuffle request that posts the given raw JSON body.
    /// </summary>
    /// <param name="jsonBody">The raw JSON body to send.</param>
    /// <returns>The configured Web request.</returns>
    private static HttpRequestMessage CreateRawShuffleRequest(string jsonBody)
    {
        HttpRequestMessage shuffleRequest = new(HttpMethod.Post, SHUFFLE_TRACKS_PATH)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        shuffleRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        shuffleRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return shuffleRequest;
    }
}
