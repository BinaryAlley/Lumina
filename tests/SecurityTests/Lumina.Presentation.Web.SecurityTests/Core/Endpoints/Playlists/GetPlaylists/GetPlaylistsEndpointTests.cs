#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylists;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// Contains security tests for the <see cref="GetPlaylistsEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route declares no rate limiter policy, so rate limiting is not exercised here. Timing attack resistance does not apply because the route
/// performs no credential comparison, and token or claim correctness does not apply because the Web layer issues no token for this route.
/// </remarks>
[ExcludeFromCodeCoverage]
public class GetPlaylistsEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string UPSTREAM_ENDPOINT = "users/me/playlists";

    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaylistSummaryDtoFixture _playlistSummaryDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly string _clientIp;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPlaylistsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _clientIp = $"10.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 254)}";
    }

    [Fact]
    public async Task GetPlaylists_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/playlists/api-get-playlists");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetPlaylists_WhenCalledByAuthenticatedUser_ShouldBePrivateAndNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse(UPSTREAM_ENDPOINT, new[] { _playlistSummaryDtoFixture.Create() });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/playlists/api-get-playlists");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("other-user-id")] // a plain identifier of another user
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Playlists--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    public async Task GetPlaylists_WithAnotherUserIdInTheQueryString_ShouldKeepTheUsersMeScope(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse(UPSTREAM_ENDPOINT, new[] { _playlistSummaryDtoFixture.Create() });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/api-get-playlists?userId={Uri.EscapeDataString(payload)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetPlaylistEndpointCalls();
        Assert.Equal([UPSTREAM_ENDPOINT], endpointCalls);
        Assert.All(endpointCalls, endpoint => Assert.StartsWith("users/me/", endpoint, StringComparison.Ordinal));
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPlaylists_WhenApiReportsForbidden_ShouldReturnASanitizedErrorWithoutLeakingAnotherUsersPlaylists()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetException(UPSTREAM_ENDPOINT, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Forbidden", detail: "PlaylistsAccessDenied"),
            HttpStatusCode.Forbidden,
            UPSTREAM_ENDPOINT));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/playlists/api-get-playlists");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
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
    /// Gets the playlist endpoints that were called upstream, excluding the endpoints called by the authentication helpers.
    /// </summary>
    /// <returns>The list of upstream playlist endpoints that were called.</returns>
    private List<string> GetPlaylistEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.GetEndpointsCalled
            .Where(endpoint => endpoint == UPSTREAM_ENDPOINT)];
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
