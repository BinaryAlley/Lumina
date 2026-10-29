#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;

/// <summary>
/// Contains integration tests for the <see cref="StreamMusicTrackSegmentEndpoint"/> class.
/// </summary>
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
    public async Task StreamMusicTrackSegment_WhenRequestIsValid_ShouldProxyTheApiSegment()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string deliveryId = Guid.NewGuid().ToString("N");
        const int SEGMENT_INDEX = 3;
        string expectedEndpoint = BuildExpectedEndpoint(libraryId, artistId, albumId, trackId, deliveryId, SEGMENT_INDEX);
        byte[] contentBytes = CreateBytes(1024);
        _apiFactory.StreamingApiClientStub.RegisterStreamResponse(expectedEndpoint, await CreateStreamResultAsync(contentBytes));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildBrowserPath(libraryId, artistId, albumId, trackId, deliveryId, SEGMENT_INDEX));
        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal("audio/mp4", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(contentBytes.Length, response.Content.Headers.ContentLength);
        Assert.Equal(contentBytes, responseBytes);
        Assert.Equal("9", response.Headers.GetValues(ApiStreamResult.SEGMENT_COUNT_HEADER_NAME).Single());
        Assert.Equal("6.5", response.Headers.GetValues(ApiStreamResult.SEGMENT_START_SECONDS_HEADER_NAME).Single());
        Assert.Equal("300", response.Headers.GetValues(ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME).Single());
        Assert.Contains("private", response.Headers.CacheControl?.ToString());
        Assert.DoesNotContain("public", response.Headers.CacheControl?.ToString());
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.Single(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
        Assert.Equal(expectedEndpoint, _apiFactory.StreamingApiClientStub.GetEndpointsCalled[0]);
    }

    [Fact]
    public async Task StreamMusicTrackSegment_WhenDeliveryIdIsInvalid_ShouldReturnNotFoundWithoutCallingTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildBrowserPath(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString("N").ToUpperInvariant(), 0));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(string.Empty, content);
        Assert.Empty(_apiFactory.StreamingApiClientStub.GetEndpointsCalled);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)] // a busy normalization pipeline is a transient condition
    [InlineData(HttpStatusCode.TooManyRequests)] // the stream concurrency limit was reached
    public async Task StreamMusicTrackSegment_WhenApiReportsATransientCondition_ShouldReturnABareStatus(HttpStatusCode statusCode)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.StreamingApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string deliveryId = Guid.NewGuid().ToString("N");
        string expectedEndpoint = BuildExpectedEndpoint(libraryId, artistId, albumId, trackId, deliveryId, 0);
        _apiFactory.StreamingApiClientStub.RegisterStreamException(expectedEndpoint, new ApiException(
            new ProblemDetailsDto { Title = "General.Failure", Detail = "InternalServerError", Status = (int)statusCode },
            statusCode,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(BuildBrowserPath(libraryId, artistId, albumId, trackId, deliveryId, 0));
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(string.Empty, content);
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
    /// Builds the upstream endpoint the segment request is expected to be sent to.
    /// </summary>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The Id of the artist the album belongs to.</param>
    /// <param name="albumId">The Id of the album the track belongs to.</param>
    /// <param name="trackId">The Id of the track.</param>
    /// <param name="deliveryId">The opaque handle of the normalized delivery.</param>
    /// <param name="segmentIndex">The zero based index of the segment.</param>
    /// <returns>The expected upstream endpoint.</returns>
    private static string BuildExpectedEndpoint(Guid libraryId, Guid artistId, Guid albumId, Guid trackId, string deliveryId, int segmentIndex)
    {
        return ApiRoutes.Music.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", libraryId.ToString())
            .Replace("{artistId}", artistId.ToString())
            .Replace("{albumId}", albumId.ToString())
            .Replace("{trackId}", trackId.ToString())
            .Replace("{deliveryId}", deliveryId)
            .Replace("{segmentIndex}", segmentIndex.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Creates a streamed API response with a partial content status, a body and the segment headers.
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
        response.Headers.CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromSeconds(60) };
        response.Headers.TryAddWithoutValidation(ApiStreamResult.SEGMENT_COUNT_HEADER_NAME, "9");
        response.Headers.TryAddWithoutValidation(ApiStreamResult.SEGMENT_START_SECONDS_HEADER_NAME, "6.5");
        response.Headers.TryAddWithoutValidation(ApiStreamResult.TRACK_DURATION_SECONDS_HEADER_NAME, "300");
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
