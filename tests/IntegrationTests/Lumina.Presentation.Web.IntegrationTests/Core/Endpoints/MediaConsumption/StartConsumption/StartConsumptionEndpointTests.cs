#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.StartConsumption;
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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Contains integration tests for the <see cref="StartConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointTests : IClassFixture<LuminaWebFactory>
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_libraryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_mediaItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly LuminaWebFactory _apiFactory;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();
    private readonly MediaPositionDtoFixture _mediaPositionDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StartConsumptionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StartConsumption_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldStartSessionThroughApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"libraries/{s_libraryId}/media-items/MusicTrack/{s_mediaItemId}/consumption-records";
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(
            libraryId: s_libraryId,
            mediaItemId: s_mediaItemId,
            mediaKind: MediaKind.MusicTrack,
            status: ConsumptionStatus.InProgress);
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, expectedRecord);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.MusicTrack), s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedRecord.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(expectedRecord.LibraryId, data.GetProperty("libraryId").GetGuid());
        Assert.Equal(expectedRecord.MediaItemId, data.GetProperty("mediaItemId").GetGuid());
        Assert.Equal(nameof(MediaKind.MusicTrack), data.GetProperty("mediaKind").GetString());
        Assert.Equal(nameof(ConsumptionStatus.InProgress), data.GetProperty("status").GetString());
        Assert.Equal([expectedEndpoint], GetConsumptionEndpointCalls());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task StartConsumption_WhenMediaKindIsLowercase_ShouldForwardTheCanonicalMemberName()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"libraries/{s_libraryId}/media-items/MusicTrack/{s_mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), "musictrack", s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetConsumptionEndpointCalls());
    }

    [Fact]
    public async Task StartConsumption_WhenMediaKindIsUnknown_ShouldForwardTheSanitizedPlaceholder()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"libraries/{s_libraryId}/media-items/Invalid/{s_mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), "notamediakind", s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetConsumptionEndpointCalls());
        Assert.DoesNotContain("notamediakind", string.Join(",", GetConsumptionEndpointCalls()), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartConsumption_WhenApiReturnsAResumePoint_ShouldSurfaceThePositionAndProgressUnchanged()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"libraries/{s_libraryId}/media-items/Book/{s_mediaItemId}/consumption-records";
        MediaPositionDto lastPosition = _mediaPositionDtoFixture.Create(
            kind: MediaPositionKind.TimeOffset,
            timeOffsetSeconds: 123.5,
            includeLocation: false);
        ConsumptionRecordDto expectedRecord = _consumptionRecordDtoFixture.Create(
            libraryId: s_libraryId,
            mediaItemId: s_mediaItemId,
            mediaKind: MediaKind.Book,
            status: ConsumptionStatus.InProgress,
            lastPosition: lastPosition,
            lastProgressFraction: 0.37,
            consecutivePagesRead: 21);
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, expectedRecord);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.Book), s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement data = json.RootElement.GetProperty("data");
        JsonElement position = data.GetProperty("lastPosition");
        Assert.Equal(nameof(MediaPositionKind.TimeOffset), position.GetProperty("kind").GetString());
        Assert.Equal(123.5, position.GetProperty("timeOffsetSeconds").GetDouble());
        Assert.Equal(0.37, data.GetProperty("lastProgressFraction").GetDouble());
        Assert.Equal(21, data.GetProperty("consecutivePagesRead").GetInt32());
    }

    [Fact]
    public async Task StartConsumption_WhenIdentifiersAreNotGuids_ShouldForwardTheEmptyGuidForEach()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"libraries/{ZERO_GUID}/media-items/MusicTrack/{ZERO_GUID}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest("not-a-guid", nameof(MediaKind.MusicTrack), "not-a-guid", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetConsumptionEndpointCalls();
        Assert.Equal([sanitizedEndpoint], endpointCalls);
        Assert.DoesNotContain("not-a-guid", string.Join(",", endpointCalls), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartConsumption_WhenRouteIdentifiersAreHexGuidSegments_ShouldStillResolveThem()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        Guid mediaItemId = Guid.Parse("FEDCBAFE-DCBA-FEDC-BAFE-DCBAFEDCBAFE");
        string uppercaseLibraryId = libraryId.ToString().ToUpperInvariant();
        string uppercaseMediaItemId = mediaItemId.ToString().ToUpperInvariant();
        string expectedEndpoint = $"libraries/{libraryId}/media-items/MusicTrack/{mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(uppercaseLibraryId, nameof(MediaKind.MusicTrack), uppercaseMediaItemId, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetConsumptionEndpointCalls();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.DoesNotContain(uppercaseLibraryId, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(uppercaseMediaItemId, string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the consumption endpoints that were called upstream, excluding the login request performed to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream consumption endpoints that were called.</returns>
    private List<string> GetConsumptionEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint != ApiRoutes.Authentication.LOGIN_ACCOUNT)
            .Select(postRequest => postRequest.Endpoint)];
    }

    /// <summary>
    /// Builds the start consumption request for the specified route values.
    /// </summary>
    /// <param name="libraryId">The value of the library identifier route segment.</param>
    /// <param name="mediaKind">The value of the media kind route segment.</param>
    /// <param name="mediaItemId">The value of the media item identifier route segment.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured start consumption request.</returns>
    private static HttpRequestMessage CreateStartRequest(string libraryId, string mediaKind, string mediaItemId, string antiforgeryToken)
    {
        HttpRequestMessage startRequest = new(HttpMethod.Post, $"/en-us/media-consumption/api-start-consumption/{libraryId}/{mediaKind}/{mediaItemId}")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        startRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        startRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        startRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return startRequest;
    }
}
