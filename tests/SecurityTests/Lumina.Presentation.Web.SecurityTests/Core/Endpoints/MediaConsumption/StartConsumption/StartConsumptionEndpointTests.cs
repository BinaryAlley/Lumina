#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.MediaConsumption;
using Lumina.Presentation.Web.Common.Enums.MediaConsumption;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.MediaConsumption.StartConsumption;
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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// Contains security tests for the <see cref="StartConsumptionEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class StartConsumptionEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private static readonly Guid s_libraryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid s_mediaItemId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly LuminaWebFactory _apiFactory;
    private readonly ConsumptionRecordDtoFixture _consumptionRecordDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public StartConsumptionEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task StartConsumption_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.MusicTrack), s_mediaItemId.ToString(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetConsumptionEndpointCalls());
    }

    [Fact]
    public async Task StartConsumption_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.MusicTrack), s_mediaItemId.ToString(), null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetConsumptionEndpointCalls());
    }

    [Fact]
    public async Task StartConsumption_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.MusicTrack), s_mediaItemId.ToString(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetConsumptionEndpointCalls());
    }

    [Fact]
    public async Task StartConsumption_WhenCalledByAuthenticatedUserWithValidToken_ShouldCallApiOnceWithoutLeakingSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"libraries/{s_libraryId}/media-items/MusicTrack/{s_mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _consumptionRecordDtoFixture.Create(mediaKind: MediaKind.MusicTrack));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.MusicTrack), s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expectedEndpoint], GetConsumptionEndpointCalls());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    [InlineData("MusicTrack/../../admin")] // path traversal through a valid media kind prefix
    public async Task StartConsumption_WithInjectionPayloadInMediaKind_ShouldSanitizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"libraries/{s_libraryId}/media-items/Invalid/{s_mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), Uri.EscapeDataString(payload), s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> endpointCalls = GetConsumptionEndpointCalls();
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
    public async Task StartConsumption_WithInjectionPayloadInLibraryId_ShouldUseTheEmptyGuidAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"libraries/{ZERO_GUID}/media-items/MusicTrack/{s_mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(Uri.EscapeDataString(payload), nameof(MediaKind.MusicTrack), s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetConsumptionEndpointCalls();
        Assert.Equal([sanitizedEndpoint], endpointCalls);
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task StartConsumption_WithInjectionPayloadInMediaItemId_ShouldUseTheEmptyGuidAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"libraries/{s_libraryId}/media-items/MusicTrack/{ZERO_GUID}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.MusicTrack), Uri.EscapeDataString(payload), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetConsumptionEndpointCalls();
        Assert.Equal([sanitizedEndpoint], endpointCalls);
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Fact]
    public async Task StartConsumption_WhenMediaKindContainsAnEncodedSlash_ShouldNeverForwardItRaw()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string CRAFTED_MEDIA_KIND = "MusicTrack/../..";
        string sanitizedEndpoint = $"libraries/{s_libraryId}/media-items/Invalid/{s_mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _consumptionRecordDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), Uri.EscapeDataString(CRAFTED_MEDIA_KIND), s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain(CRAFTED_MEDIA_KIND, string.Join(",", GetConsumptionEndpointCalls()), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(CRAFTED_MEDIA_KIND), string.Join(",", GetConsumptionEndpointCalls()), StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.All(GetConsumptionEndpointCalls(), endpoint => Assert.Equal(sanitizedEndpoint, endpoint));
    }

    [Fact]
    public async Task StartConsumption_WhenApiFails_ShouldNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"libraries/{s_libraryId}/media-items/MusicTrack/{s_mediaItemId}/consumption-records";
        _apiFactory.ApiClientStub.RegisterPostException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage request = CreateStartRequest(s_libraryId.ToString(), nameof(MediaKind.MusicTrack), s_mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
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
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured start consumption request.</returns>
    private static HttpRequestMessage CreateStartRequest(string libraryId, string mediaKind, string mediaItemId, string? antiforgeryToken)
    {
        HttpRequestMessage startRequest = new(HttpMethod.Post, $"/en-us/media-consumption/api-start-consumption/{libraryId}/{mediaKind}/{mediaItemId}")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        startRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        startRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            startRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return startRequest;
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
