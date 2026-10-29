#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Contains security tests for the <see cref="GetPlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPlaylist_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/en-us/playlists/item/{Guid.NewGuid()}/api-get-playlist");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetPlaylist_WhenCalledByAuthenticatedUser_ShouldBePrivateAndNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _playlistDtoFixture.Create(id: playlistId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/item/{playlistId}/api-get-playlist");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetPlaylistEndpointCalls());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPlaylist_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuidAndNotTheRawValue()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(sanitizedEndpoint, _playlistDtoFixture.Create(id: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/playlists/item/not-a-guid/api-get-playlist");

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetPlaylistEndpointCalls();
        Assert.Equal([sanitizedEndpoint], endpointCalls);
        Assert.DoesNotContain("not-a-guid", string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("1;DROP TABLE")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // markup injection
    [InlineData("𝕡𝕝𝕒𝕪𝕝𝕚𝕤𝕥")] // unicode payload
    public async Task GetPlaylist_WithInjectionPayloadInPlaylistId_ShouldNormalizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(sanitizedEndpoint, _playlistDtoFixture.Create(id: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/item/{Uri.EscapeDataString(payload)}/api-get-playlist");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> endpointCalls = GetPlaylistEndpointCalls();
        Assert.All(endpointCalls, endpoint => Assert.Equal(sanitizedEndpoint, endpoint));
        Assert.DoesNotContain(payload, string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", endpointCalls), StringComparison.Ordinal);
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
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
            .Where(endpoint => endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal))];
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
