#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;

/// <summary>
/// Contains integration tests for the <see cref="StreamMusicTrackEndpoint"/> class.
/// </summary>
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
    public async Task StreamMusicTrack_WhenRequestIsValid_ShouldProxyTheApiStream()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string expectedEndpoint = BuildExpectedEndpoint(libraryId, artistId, albumId, trackId);
        byte[] contentBytes = CreateBytes(2048);
        _apiFactory.StreamingApiClientStub.RegisterStreamResponse(expectedEndpoint, await CreateStreamResultAsync(HttpStatusCode.OK, contentBytes, "audio/mpeg"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildBrowserPath(libraryId, artistId, albumId, trackId));
        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("audio/mpeg", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(contentBytes.Length, response.Content.Headers.ContentLength);
        Assert.Equal(contentBytes, responseBytes);
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.DoesNotContain("public", response.Headers.CacheControl?.ToString());
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.Single(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
        Assert.Equal(expectedEndpoint, _apiFactory.StreamingApiClientStub.GetEndpointsCalled[0]);
    }

    [Fact]
    public async Task StreamMusicTrack_WhenConditionalHeadersArePresent_ShouldForwardThemToTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string expectedEndpoint = BuildExpectedEndpoint(libraryId, artistId, albumId, trackId);
        _apiFactory.StreamingApiClientStub.RegisterStreamResponse(expectedEndpoint, await CreateStreamResultAsync(HttpStatusCode.PartialContent, CreateBytes(100), "audio/mpeg"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        using HttpRequestMessage request = new(HttpMethod.Get, BuildBrowserPath(libraryId, artistId, albumId, trackId));
        request.Headers.Range = new RangeHeaderValue(100, 199);
        request.Headers.IfRange = new RangeConditionHeaderValue(new EntityTagHeaderValue("\"etag-value\""));
        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue("\"etag-value\""));
        request.Headers.IfModifiedSince = new DateTimeOffset(2015, 10, 21, 7, 28, 0, TimeSpan.Zero);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Single(_apiFactory.StreamingApiClientStub.StreamRequests);
        IReadOnlyDictionary<string, string> forwardedHeaders = _apiFactory.StreamingApiClientStub.StreamRequests[0].Headers;
        Assert.Equal("bytes=100-199", forwardedHeaders["Range"]);
        Assert.Equal("\"etag-value\"", forwardedHeaders["If-Range"]);
        Assert.Equal("\"etag-value\"", forwardedHeaders["If-None-Match"]);
        Assert.Equal("Wed, 21 Oct 2015 07:28:00 GMT", forwardedHeaders["If-Modified-Since"]);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)] // a busy normalization pipeline is a transient condition
    [InlineData(HttpStatusCode.TooManyRequests)] // the stream concurrency limit was reached
    public async Task StreamMusicTrack_WhenApiReportsATransientCondition_ShouldReturnABareStatus(HttpStatusCode statusCode)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string expectedEndpoint = BuildExpectedEndpoint(libraryId, artistId, albumId, trackId);
        _apiFactory.StreamingApiClientStub.RegisterStreamException(expectedEndpoint, new ApiException(
            new ProblemDetailsDto { Title = "General.Failure", Detail = "InternalServerError", Status = (int)statusCode },
            statusCode,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        using HttpRequestMessage request = new(HttpMethod.Get, BuildBrowserPath(libraryId, artistId, albumId, trackId));
        // A media caller asks for audio, which is what makes the failure a bare status rather than a JSON or page response.
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(string.Empty, content);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)] // a busy normalization pipeline is a transient condition
    [InlineData(HttpStatusCode.TooManyRequests)] // the stream concurrency limit was reached
    public async Task StreamMusicTrack_WhenApiReportsATransientConditionForANonMediaCaller_ShouldStillReturnABareStatus(HttpStatusCode statusCode)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string expectedEndpoint = BuildExpectedEndpoint(libraryId, artistId, albumId, trackId);
        _apiFactory.StreamingApiClientStub.RegisterStreamException(expectedEndpoint, new ApiException(
            new ProblemDetailsDto { Title = "General.Failure", Detail = "InternalServerError", Status = (int)statusCode },
            statusCode,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        // No Accept and no Range header: a caller that is not asking for media must still receive the bare transient status, not a
        // JSON body with status 200 that the navigation error middleware would otherwise produce.
        using HttpRequestMessage request = new(HttpMethod.Get, BuildBrowserPath(libraryId, artistId, albumId, trackId));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(string.Empty, content);
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
    /// Builds the upstream endpoint the stream request is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId, Guid trackId)
    {
        return ApiRoutes.Music.STREAM_TRACK
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString());
    }

    /// <summary>
    /// Creates a streamed API response with a status code, a body and a media type.
    /// </summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="contentBytes">The bytes of the content of the response.</param>
    /// <param name="contentType">The media type of the content.</param>
    /// <returns>The created streamed response.</returns>
    private static async Task<ApiStreamResult> CreateStreamResultAsync(HttpStatusCode statusCode, byte[] contentBytes, string contentType)
    {
        HttpResponseMessage response = new(statusCode)
        {
            Content = new ByteArrayContent(contentBytes)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        response.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60) };
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
