#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.UpdatePlaylist;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// Contains security tests for the <see cref="UpdatePlaylistEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route declares no rate limiter policy, so rate limiting is not exercised here. Timing attack resistance does not apply because the route
/// performs no credential comparison, and token or claim correctness does not apply because the Web layer issues no token for this route.
/// </remarks>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly UpdatePlaylistRequestFixture _updatePlaylistRequestFixture = new();
    private readonly PlaylistDetailsDtoFixture _playlistDetailsDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly string _clientIp;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdatePlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _clientIp = $"10.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 254)}";
    }

    [Fact]
    public async Task UpdatePlaylist_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage updateRequest = CreateUpdateRequest(Guid.NewGuid().ToString(), _updatePlaylistRequestFixture.Create(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetUpdateRequests());
    }

    [Fact]
    public async Task UpdatePlaylist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage updateRequest = CreateUpdateRequest(Guid.NewGuid().ToString(), _updatePlaylistRequestFixture.Create(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetUpdateRequests());
    }

    [Fact]
    public async Task UpdatePlaylist_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage updateRequest = CreateUpdateRequest(Guid.NewGuid().ToString(), _updatePlaylistRequestFixture.Create(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetUpdateRequests());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task UpdatePlaylist_WithInjectionPayloadInPlaylistId_ShouldNormalizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/playlists/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterPutResponse(sanitizedEndpoint, _playlistDetailsDtoFixture.Create(id: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage updateRequest = CreateUpdateRequest(Uri.EscapeDataString(payload), _updatePlaylistRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> endpointCalls = GetUpdateRequests().Select(updateRequestCall => updateRequestCall.Endpoint).ToList();
        Assert.All(endpointCalls, endpoint => Assert.Equal(sanitizedEndpoint, endpoint));
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task UpdatePlaylist_WithInjectionPayloadInName_ShouldNotFailOrLeakThePayload(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, _playlistDetailsDtoFixture.Create(id: playlistId, name: "Updated playlist"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage updateRequest = CreateUpdateRequest(playlistId.ToString(), _updatePlaylistRequestFixture.Create(name: payload), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
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

    [Fact]
    public async Task UpdatePlaylist_WhenBodyTriesToTargetAnotherUser_ShouldKeepTheUsersMeScope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, _playlistDetailsDtoFixture.Create(id: playlistId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage updateRequest = CreateRawUpdateRequest(
            playlistId.ToString(),
            $"{{\"name\": \"Updated playlist\", \"userId\": \"{otherUserId}\"}}",
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetUpdateRequests().Select(updateRequestCall => updateRequestCall.Endpoint).ToList();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain(otherUserId.ToString(), string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenPlaylistBelongsToAnotherUser_ShouldKeepTheUsersMeScopeAndNotLeakData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid otherUsersPlaylistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{otherUsersPlaylistId}";
        _apiFactory.ApiClientStub.RegisterPutException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Forbidden", detail: "PlaylistAccessDenied"),
            HttpStatusCode.Forbidden,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage updateRequest = CreateUpdateRequest(otherUsersPlaylistId.ToString(), _updatePlaylistRequestFixture.Create(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        List<string> endpointCalls = GetUpdateRequests().Select(updateRequestCall => updateRequestCall.Endpoint).ToList();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the update playlist requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream update playlist requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetUpdateRequests()
    {
        return [.. _apiFactory.ApiClientStub.PutRequests
            .Where(putRequest => putRequest.Endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the update playlist request for the specified route value.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured update playlist request.</returns>
    private static HttpRequestMessage CreateUpdateRequest(string playlistId, UpdatePlaylistRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage updateRequest = new(HttpMethod.Put, $"/en-us/playlists/item/{playlistId}/api-update-playlist")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return updateRequest;
    }

    /// <summary>
    /// Builds the update playlist request that posts the given raw JSON body.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="jsonBody">The raw JSON body to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured update playlist request.</returns>
    private static HttpRequestMessage CreateRawUpdateRequest(string playlistId, string jsonBody, string antiforgeryToken)
    {
        HttpRequestMessage updateRequest = new(HttpMethod.Put, $"/en-us/playlists/item/{playlistId}/api-update-playlist")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        updateRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return updateRequest;
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
