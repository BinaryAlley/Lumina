#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.DeletePlaylist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.DeletePlaylist;

/// <summary>
/// Contains security tests for the <see cref="DeletePlaylistEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route declares no rate limiter policy, so rate limiting is not exercised here. Timing attack resistance does not apply because the route
/// performs no credential comparison, and token or claim correctness does not apply because the Web layer issues no token for this route.
/// </remarks>
[ExcludeFromCodeCoverage]
public class DeletePlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly string _clientIp;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeletePlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _clientIp = $"10.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 254)}";
    }

    [Fact]
    public async Task DeletePlaylist_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Guid.NewGuid().ToString(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetDeleteEndpointCalls());
    }

    [Fact]
    public async Task DeletePlaylist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Guid.NewGuid().ToString(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetDeleteEndpointCalls());
    }

    [Fact]
    public async Task DeletePlaylist_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Guid.NewGuid().ToString(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetDeleteEndpointCalls());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    public async Task DeletePlaylist_WithInjectionPayloadInPlaylistId_ShouldNormalizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/playlists/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(sanitizedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Uri.EscapeDataString(payload), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> endpointCalls = GetDeleteEndpointCalls();
        Assert.All(endpointCalls, endpoint => Assert.Equal(sanitizedEndpoint, endpoint));
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeletePlaylist_WhenPlaylistBelongsToAnotherUser_ShouldKeepTheUsersMeScopeAndNotLeakData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid otherUsersPlaylistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{otherUsersPlaylistId}";
        _apiFactory.ApiClientStub.RegisterDeleteException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Forbidden", detail: "PlaylistAccessDenied"),
            HttpStatusCode.Forbidden,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(otherUsersPlaylistId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        List<string> endpointCalls = GetDeleteEndpointCalls();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the delete playlist endpoints that were called upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream delete playlist endpoints that were called.</returns>
    private List<string> GetDeleteEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.DeleteEndpointsCalled
            .Where(endpoint => endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the delete playlist request for the specified route value.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured delete playlist request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(string playlistId, string? antiforgeryToken)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/playlists/item/{playlistId}/api-delete-playlist")
        {
            // The front-end sends the JSON content type even for DELETE requests, which triggers the antiforgery validation.
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        deleteRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            deleteRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return deleteRequest;
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
