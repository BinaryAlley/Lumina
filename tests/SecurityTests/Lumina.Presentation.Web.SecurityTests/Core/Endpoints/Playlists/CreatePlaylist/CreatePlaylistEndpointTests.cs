#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.CreatePlaylist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Contains security tests for the <see cref="CreatePlaylistEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route declares no rate limiter policy, so rate limiting is not exercised here. Timing attack resistance does not apply because the route
/// performs no credential comparison, and token or claim correctness does not apply because the Web layer issues no token for this route.
/// </remarks>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string UPSTREAM_ENDPOINT = "users/me/playlists";

    private readonly LuminaWebFactory _apiFactory;
    private readonly CreatePlaylistRequestFixture _createPlaylistRequestFixture = new();
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly string _clientIp;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CreatePlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _clientIp = $"10.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 254)}";
    }

    [Fact]
    public async Task CreatePlaylist_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage createRequest = CreateCreateRequest(_createPlaylistRequestFixture.Create(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetCreateRequests());
    }

    [Fact]
    public async Task CreatePlaylist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage createRequest = CreateCreateRequest(_createPlaylistRequestFixture.Create(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetCreateRequests());
    }

    [Fact]
    public async Task CreatePlaylist_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage createRequest = CreateCreateRequest(_createPlaylistRequestFixture.Create(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetCreateRequests());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task CreatePlaylist_WithInjectionPayloadInName_ShouldNotFailOrLeakThePayload(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostResponse(UPSTREAM_ENDPOINT, _playlistDtoFixture.Create(name: "Injected playlist"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage createRequest = CreateCreateRequest(_createPlaylistRequestFixture.Create(name: payload), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // A playlist name is a free-form string, so the Web layer forwards it and the API is responsible for storing it safely; the Web layer must never echo it back.
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    public async Task CreatePlaylist_WithInjectionPayloadInType_ShouldRejectTheRequestWithoutCallingTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage createRequest = CreateRawCreateRequest(
            $"{{\"name\": \"Test playlist\", \"type\": {JsonSerializer.Serialize(payload)}}}",
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The media type is bound to an enum, so the crafted value is rejected before the endpoint executes and can never reach the API.
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Empty(GetCreateRequests());
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreatePlaylist_WhenBodyTriesToTargetAnotherUser_ShouldKeepTheUsersMeScope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid otherUserId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPostResponse(UPSTREAM_ENDPOINT, _playlistDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage createRequest = CreateRawCreateRequest(
            $"{{\"name\": \"Test playlist\", \"type\": \"{PlaylistType.Music}\", \"userId\": \"{otherUserId}\"}}",
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetCreateRequests().Select(createRequestCall => createRequestCall.Endpoint).ToList();
        Assert.Equal([UPSTREAM_ENDPOINT], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain(otherUserId.ToString(), string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreatePlaylist_WhenApiReportsForbidden_ShouldReturnASanitizedErrorWithoutLeakingData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException(UPSTREAM_ENDPOINT, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Forbidden", detail: "PlaylistCreationForbidden"),
            HttpStatusCode.Forbidden,
            UPSTREAM_ENDPOINT));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage createRequest = CreateCreateRequest(_createPlaylistRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the create playlist requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream create playlist requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetCreateRequests()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint == UPSTREAM_ENDPOINT)];
    }

    /// <summary>
    /// Builds the create playlist request that sends the given <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured create playlist request.</returns>
    private static HttpRequestMessage CreateCreateRequest(CreatePlaylistRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/playlists/api-create-playlist")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        createRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            createRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return createRequest;
    }

    /// <summary>
    /// Builds the create playlist request that posts the given raw JSON body.
    /// </summary>
    /// <param name="jsonBody">The raw JSON body to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured create playlist request.</returns>
    private static HttpRequestMessage CreateRawCreateRequest(string jsonBody, string antiforgeryToken)
    {
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/playlists/api-create-playlist")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        createRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        createRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return createRequest;
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
