#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.GetResumePoint;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.MediaConsumption.GetResumePoint;

/// <summary>
/// Contains integration tests for the <see cref="GetResumePointEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";
    private const string RESUME_POINT_ENDPOINT_PREFIX = "users/me/media-resume-points/";

    private readonly LuminaWebFactory _apiFactory;
    private readonly ResumePointDtoFixture _resumePointDtoFixture = new();
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetResumePointEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetResumePoint_WhenApiReturnsABookResumePoint_ShouldReturnItWithEveryFieldAndPrivateCaching()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid resumePointId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Book/{mediaItemId}";
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.Location,
            location: "epubcfi(/6/4!/2/2)",
            includeTimeOffsetSeconds: false);
        ResumePointDto expectedResumePoint = _resumePointDtoFixture.Create(
            id: resumePointId,
            libraryId: libraryId,
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Book,
            lastPosition: lastPosition,
            lastProgressFraction: 0.37);
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedResumePoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest("Book", mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedResumePoint.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(expectedResumePoint.LibraryId, data.GetProperty("libraryId").GetGuid());
        Assert.Equal(expectedResumePoint.MediaItemId, data.GetProperty("mediaItemId").GetGuid());
        Assert.Equal(nameof(MediaKind.Book), data.GetProperty("mediaKind").GetString());
        JsonElement position = data.GetProperty("lastPosition");
        Assert.Equal(nameof(MediaPositionKind.Location), position.GetProperty("kind").GetString());
        Assert.Equal(JsonValueKind.Null, position.GetProperty("timeOffsetSeconds").ValueKind);
        Assert.Equal(lastPosition.Location, position.GetProperty("location").GetString());
        Assert.Equal(expectedResumePoint.LastProgressFraction, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(expectedResumePoint.LastConsumedOnUtc, data.GetProperty("lastConsumedOnUtc").GetDateTime());
        Assert.Equal([expectedEndpoint], GetResumePointCalls());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task GetResumePoint_WhenApiReturnsAMusicTrackResumePoint_ShouldReturnItsTimeOffsetPosition()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}MusicTrack/{mediaItemId}";
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetSeconds: 321.5,
            includeLocation: false);
        ResumePointDto expectedResumePoint = _resumePointDtoFixture.Create(
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.MusicTrack,
            lastPosition: lastPosition);
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedResumePoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest("MusicTrack", mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(nameof(MediaKind.MusicTrack), data.GetProperty("mediaKind").GetString());
        JsonElement position = data.GetProperty("lastPosition");
        Assert.Equal(nameof(MediaPositionKind.TimeOffset), position.GetProperty("kind").GetString());
        Assert.Equal(321.5, position.GetProperty("timeOffsetSeconds").GetDouble());
        Assert.Equal(JsonValueKind.Null, position.GetProperty("location").ValueKind);
        Assert.Equal([expectedEndpoint], GetResumePointCalls());
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaKindIsLowercase_ShouldForwardTheCanonicalMemberName()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}MusicTrack/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _resumePointDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest("musictrack", mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expectedEndpoint], GetResumePointCalls());
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaKindIsUnknown_ShouldForwardTheSanitizedPlaceholder()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Invalid/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _resumePointDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest("notamediakind", mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expectedEndpoint], GetResumePointCalls());
        Assert.DoesNotContain("notamediakind", string.Join(",", GetResumePointCalls()), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaItemIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}MusicTrack/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterGetResponse(sanitizedEndpoint, _resumePointDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest("MusicTrack", "not-a-guid");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([sanitizedEndpoint], GetResumePointCalls());
        Assert.DoesNotContain("not-a-guid", string.Join(",", GetResumePointCalls()), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetResumePoint_WhenRouteIdentifiersAreHexGuidSegments_ShouldCanonicalizeThem()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.Parse("FEDCBAFE-DCBA-FEDC-BAFE-DCBAFEDCBAFE");
        string uppercaseMediaItemId = mediaItemId.ToString().ToUpperInvariant();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}MusicTrack/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _resumePointDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.MusicTrack), uppercaseMediaItemId);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expectedEndpoint], GetResumePointCalls());
        Assert.DoesNotContain(uppercaseMediaItemId, string.Join(",", GetResumePointCalls()), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("MediaResumePointNotFound")] // the user has no resume point for the media item
    [InlineData("BookNotFound")] // the book of the resume point no longer exists
    [InlineData("TrackNotFound")] // the track of the resume point no longer exists
    public async Task GetResumePoint_WhenApiReportsAnAbsentResumePoint_ShouldReturnSuccessWithNullData(string detail)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Book/{mediaItemId}";
        ApiException apiException = new(_problemDetailsDtoFixture.Create(detail: detail), HttpStatusCode.NotFound);
        _apiFactory.ApiClientStub.RegisterGetException(expectedEndpoint, apiException);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.Book), mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("data").ValueKind);
        Assert.Equal([expectedEndpoint], GetResumePointCalls());
    }

    [Fact]
    public async Task GetResumePoint_WhenApiReportsAnUnrelatedNotFound_ShouldNotDisguiseItAsAnAbsentResumePoint()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Book/{mediaItemId}";
        ApiException apiException = new(_problemDetailsDtoFixture.Create(detail: "RouteNotFound"), HttpStatusCode.NotFound);
        _apiFactory.ApiClientStub.RegisterGetException(expectedEndpoint, apiException);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.Book), mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NotFound", json.RootElement.GetProperty("errorMessage").GetString());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task GetResumePoint_WhenCalled_ShouldCallTheUpstreamEndpointExactlyOnce()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}PodcastEpisode/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _resumePointDtoFixture.Create(mediaKind: MediaKind.PodcastEpisode));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.PodcastEpisode), mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(GetResumePointCalls());
        Assert.Equal(expectedEndpoint, GetResumePointCalls()[0]);
    }

    /// <summary>
    /// Gets the resume point endpoints that were called upstream, excluding the login, initialization and authorization requests.
    /// </summary>
    /// <returns>The list of upstream resume point endpoints that were called.</returns>
    private List<string> GetResumePointCalls()
    {
        return [.. _apiFactory.ApiClientStub.GetEndpointsCalled
            .Where(endpoint => endpoint.StartsWith(RESUME_POINT_ENDPOINT_PREFIX, StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the get resume point request for the specified route values.
    /// </summary>
    /// <param name="mediaKind">The value of the media kind route segment.</param>
    /// <param name="mediaItemId">The value of the media item identifier route segment.</param>
    /// <returns>The configured get resume point request.</returns>
    private static HttpRequestMessage CreateGetRequest(string mediaKind, string mediaItemId)
    {
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/media-consumption/api-get-resume-point/{mediaKind}/{mediaItemId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return getRequest;
    }
}
