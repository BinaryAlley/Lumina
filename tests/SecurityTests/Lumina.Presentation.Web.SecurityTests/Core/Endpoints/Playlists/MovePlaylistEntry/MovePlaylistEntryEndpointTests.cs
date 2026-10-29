#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.MovePlaylistEntry;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// Contains security tests for the <see cref="MovePlaylistEntryEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route declares no rate limiter policy, so rate limiting is not exercised here. Timing attack resistance does not apply because the route
/// performs no credential comparison, and token or claim correctness does not apply because the Web layer issues no token for this route.
/// </remarks>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly MovePlaylistEntryRequestFixture _movePlaylistEntryRequestFixture = new();
    private readonly PlaylistEntryMovedDtoFixture _playlistEntryMovedDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly string _clientIp;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public MovePlaylistEntryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _clientIp = $"10.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 254)}";
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage moveRequest = CreateMoveRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), _movePlaylistEntryRequestFixture.Create(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(moveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetMoveRequests());
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage moveRequest = CreateMoveRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), _movePlaylistEntryRequestFixture.Create(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetMoveRequests());
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage moveRequest = CreateMoveRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), _movePlaylistEntryRequestFixture.Create(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetMoveRequests());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task MovePlaylistEntry_WithInjectionPayloadInRouteIds_ShouldNormalizeThemAndNotForwardTheRawValues(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}/move";
        _apiFactory.ApiClientStub.RegisterPutResponse(sanitizedEndpoint, _playlistEntryMovedDtoFixture.Create(playlistId: Guid.Empty, entryId: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage moveRequest = CreateMoveRequest(
            Uri.EscapeDataString(payload),
            Uri.EscapeDataString(payload),
            _movePlaylistEntryRequestFixture.Create(),
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> endpointCalls = GetMoveRequests().Select(moveRequestCall => moveRequestCall.Endpoint).ToList();
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
    public async Task MovePlaylistEntry_WithInjectionPayloadInTargetPosition_ShouldRejectTheRequestWithoutCallingTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage moveRequest = CreateRawMoveRequest(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            $"{{\"targetPosition\": {JsonSerializer.Serialize(payload)}}}",
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Empty(GetMoveRequests());
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenBodyTriesToTargetAnotherUser_ShouldKeepTheUsersMeScope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries/{playlistEntryId}/move";
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, _playlistEntryMovedDtoFixture.Create(playlistId: playlistId, entryId: playlistEntryId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage moveRequest = CreateRawMoveRequest(
            playlistId.ToString(),
            playlistEntryId.ToString(),
            $"{{\"targetPosition\": 1, \"userId\": \"{otherUserId}\"}}",
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetMoveRequests().Select(moveRequestCall => moveRequestCall.Endpoint).ToList();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain(otherUserId.ToString(), string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenPlaylistBelongsToAnotherUser_ShouldKeepTheUsersMeScopeAndNotLeakData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid otherUsersPlaylistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{otherUsersPlaylistId}/entries/{playlistEntryId}/move";
        _apiFactory.ApiClientStub.RegisterPutException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Forbidden", detail: "PlaylistAccessDenied"),
            HttpStatusCode.Forbidden,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage moveRequest = CreateMoveRequest(
            otherUsersPlaylistId.ToString(),
            playlistEntryId.ToString(),
            _movePlaylistEntryRequestFixture.Create(),
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        List<string> endpointCalls = GetMoveRequests().Select(moveRequestCall => moveRequestCall.Endpoint).ToList();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the move entry requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream move entry requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetMoveRequests()
    {
        return [.. _apiFactory.ApiClientStub.PutRequests
            .Where(putRequest => putRequest.Endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal)
                && putRequest.Endpoint.EndsWith("/move", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the move entry request for the specified route values.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="playlistEntryId">The value of the playlist entry identifier route segment.</param>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured move entry request.</returns>
    private static HttpRequestMessage CreateMoveRequest(string playlistId, string playlistEntryId, MovePlaylistEntryRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage moveRequest = new(HttpMethod.Put, $"/en-us/playlists/item/{playlistId}/entries/{playlistEntryId}/api-move-entry")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        moveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        moveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            moveRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return moveRequest;
    }

    /// <summary>
    /// Builds the move entry request that posts the given raw JSON body.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="playlistEntryId">The value of the playlist entry identifier route segment.</param>
    /// <param name="jsonBody">The raw JSON body to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured move entry request.</returns>
    private static HttpRequestMessage CreateRawMoveRequest(string playlistId, string playlistEntryId, string jsonBody, string antiforgeryToken)
    {
        HttpRequestMessage moveRequest = new(HttpMethod.Put, $"/en-us/playlists/item/{playlistId}/entries/{playlistEntryId}/api-move-entry")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        moveRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        moveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        moveRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return moveRequest;
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
