#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntries;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// Contains security tests for the <see cref="AddPlaylistEntriesEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route declares no rate limiter policy, so rate limiting is not exercised here. Timing attack resistance does not apply because the route
/// performs no credential comparison, and token or claim correctness does not apply because the Web layer issues no token for this route.
/// </remarks>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly AddPlaylistEntriesRequestFixture _addPlaylistEntriesRequestFixture = new();
    private readonly PlaylistEntriesAddedDtoFixture _playlistEntriesAddedDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly string _clientIp;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddPlaylistEntriesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _clientIp = $"10.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 254)}";
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        HttpRequestMessage addRequest = CreateAddRequest(Guid.NewGuid().ToString(), request, antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetAddRequests());
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        HttpRequestMessage addRequest = CreateAddRequest(Guid.NewGuid().ToString(), request, antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetAddRequests());
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        HttpRequestMessage addRequest = CreateAddRequest(Guid.NewGuid().ToString(), request, "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetAddRequests());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task AddPlaylistEntries_WithInjectionPayloadInPlaylistId_ShouldNormalizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/playlists/{ZERO_GUID}/entries-batch";
        _apiFactory.ApiClientStub.RegisterPostResponse(sanitizedEndpoint, _playlistEntriesAddedDtoFixture.Create(playlistId: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage addRequest = CreateAddRequest(Uri.EscapeDataString(payload), _addPlaylistEntriesRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> endpointCalls = GetAddRequests().Select(addRequestCall => addRequestCall.Endpoint).ToList();
        Assert.All(endpointCalls, endpoint => Assert.Equal(sanitizedEndpoint, endpoint));
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    public async Task AddPlaylistEntries_WithInjectionPayloadInTrackIds_ShouldRejectTheRequestWithoutCallingTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage addRequest = CreateRawAddRequest(
            Guid.NewGuid().ToString(),
            $"{{\"trackIds\": [{JsonSerializer.Serialize(payload)}]}}",
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Empty(GetAddRequests());
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenBodyTriesToTargetAnotherUser_ShouldKeepTheUsersMeScope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries-batch";
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _playlistEntriesAddedDtoFixture.Create(playlistId: playlistId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage addRequest = CreateRawAddRequest(
            playlistId.ToString(),
            $"{{\"trackIds\": [\"{Guid.NewGuid()}\"], \"userId\": \"{otherUserId}\"}}",
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetAddRequests().Select(addRequestCall => addRequestCall.Endpoint).ToList();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain(otherUserId.ToString(), string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenApiReportsForbidden_ShouldReturnASanitizedErrorWithoutLeakingData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries-batch";
        _apiFactory.ApiClientStub.RegisterPostException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Forbidden", detail: "PlaylistAccessDenied"),
            HttpStatusCode.Forbidden,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage addRequest = CreateAddRequest(playlistId.ToString(), _addPlaylistEntriesRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
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
    /// Gets the add entries requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream add entries requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetAddRequests()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal)
                && postRequest.Endpoint.EndsWith("/entries-batch", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the add entries request for the specified playlist route value.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured add entries request.</returns>
    private static HttpRequestMessage CreateAddRequest(string playlistId, AddPlaylistEntriesRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage addRequest = new(HttpMethod.Post, $"/en-us/playlists/item/{playlistId}/api-add-entries")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        addRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        addRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            addRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return addRequest;
    }

    /// <summary>
    /// Builds the add entries request that posts the given raw JSON body.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="jsonBody">The raw JSON body to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured add entries request.</returns>
    private static HttpRequestMessage CreateRawAddRequest(string playlistId, string jsonBody, string antiforgeryToken)
    {
        HttpRequestMessage addRequest = new(HttpMethod.Post, $"/en-us/playlists/item/{playlistId}/api-add-entries")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        addRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        addRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        addRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return addRequest;
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
