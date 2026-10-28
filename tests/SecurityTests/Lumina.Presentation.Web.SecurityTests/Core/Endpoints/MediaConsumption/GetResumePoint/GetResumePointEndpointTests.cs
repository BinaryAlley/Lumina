#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.GetResumePoint;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.MediaConsumption.GetResumePoint;

/// <summary>
/// Contains security tests for the <see cref="GetResumePointEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetResumePointEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";
    private const string RESUME_POINT_ENDPOINT_PREFIX = "users/me/media-resume-points/";

    private readonly LuminaWebFactory _apiFactory;
    private readonly ResumePointDtoFixture _resumePointDtoFixture = new();
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
    public async Task GetResumePoint_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string url = $"/en-us/media-consumption/api-get-resume-point/{nameof(MediaKind.MusicTrack)}/{Guid.NewGuid()}";

        // Act
        HttpResponseMessage response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetResumePointCalls());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    [InlineData("MusicTrack/../../admin")] // path traversal through a valid media kind prefix
    public async Task GetResumePoint_WithInjectionPayloadInMediaKind_ShouldSanitizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string sanitizedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Invalid/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(sanitizedEndpoint, _resumePointDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(Uri.EscapeDataString(payload), mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> endpointCalls = GetResumePointCalls();
        Assert.Equal([sanitizedEndpoint], endpointCalls);
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task GetResumePoint_WithInjectionPayloadInMediaItemId_ShouldUseTheEmptyGuidAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}MusicTrack/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterGetResponse(sanitizedEndpoint, _resumePointDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.MusicTrack), Uri.EscapeDataString(payload));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<string> endpointCalls = GetResumePointCalls();
        Assert.Equal([sanitizedEndpoint], endpointCalls);
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaKindContainsAnEncodedSlash_ShouldNeverForwardItRaw()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string CRAFTED_MEDIA_KIND = "MusicTrack/../..";
        Guid mediaItemId = Guid.NewGuid();
        string sanitizedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Invalid/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(sanitizedEndpoint, _resumePointDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(Uri.EscapeDataString(CRAFTED_MEDIA_KIND), mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(CRAFTED_MEDIA_KIND, string.Join(",", GetResumePointCalls()), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(CRAFTED_MEDIA_KIND), string.Join(",", GetResumePointCalls()), StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.All(GetResumePointCalls(), endpoint => Assert.Equal(sanitizedEndpoint, endpoint));
    }

    [Fact]
    public async Task GetResumePoint_WhenMediaKindIsNumeric_ShouldRejectItAndForwardTheInvalidPlaceholder()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string sanitizedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Invalid/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(sanitizedEndpoint, _resumePointDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest("1", mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([sanitizedEndpoint], GetResumePointCalls());
    }

    [Fact]
    public async Task GetResumePoint_WhenQueryStringIsTamperedWith_ShouldNotChangeTheUpstreamPath()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string otherUserId = Guid.NewGuid().ToString();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}MusicTrack/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _resumePointDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.MusicTrack), $"{mediaItemId}?userId={otherUserId}");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<string> endpointCalls = GetResumePointCalls();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.DoesNotContain("?", endpointCalls[0], StringComparison.Ordinal);
        Assert.DoesNotContain(otherUserId, string.Join(",", endpointCalls), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetResumePoint_WhenCalledByAuthenticatedUser_ShouldNotAllowSharedCachingAndShouldNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}Book/{mediaItemId}";
        ResumePointDto resumePoint = _resumePointDtoFixture.Create(
            mediaItemId: mediaItemId,
            mediaKind: MediaKind.Book,
            lastProgressFraction: 0.5);
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, resumePoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.Book), mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetResumePoint_WhenApiFails_ShouldNotDisguiseItAsAResumePointAndShouldNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedEndpoint = $"{RESUME_POINT_ENDPOINT_PREFIX}MusicTrack/{mediaItemId}";
        ApiException apiException = new(_problemDetailsDtoFixture.Create(), HttpStatusCode.InternalServerError);
        _apiFactory.ApiClientStub.RegisterGetException(expectedEndpoint, apiException);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = CreateGetRequest(nameof(MediaKind.MusicTrack), mediaItemId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        using JsonDocument json = await WebTestHelpers.ReadJsonAsync(response);
        string content = json.RootElement.GetRawText();

        // Assert
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
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
    /// <param name="mediaItemId">The value of the media item identifier route segment, optionally including a query string.</param>
    /// <returns>The configured get resume point request.</returns>
    private static HttpRequestMessage CreateGetRequest(string mediaKind, string mediaItemId)
    {
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/media-consumption/api-get-resume-point/{mediaKind}/{mediaItemId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return getRequest;
    }

    /// <summary>
    /// Releases the resources used by the test class and resets the remote API stub.
    /// </summary>
    public void Dispose()
    {
        _apiFactory.ApiClientStub.Reset();
        GC.SuppressFinalize(this);
    }
}
