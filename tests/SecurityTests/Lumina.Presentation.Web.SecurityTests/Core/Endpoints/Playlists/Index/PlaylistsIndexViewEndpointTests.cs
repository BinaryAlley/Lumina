#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Playlists.Index;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Playlists.Index;

/// <summary>
/// Contains security tests for the <see cref="PlaylistsIndexViewEndpoint"/> class.
/// </summary>
/// <remarks>
/// The route declares no rate limiter policy, so rate limiting is not exercised here. Timing attack resistance does not apply because the route
/// performs no credential comparison, token or claim correctness does not apply because the Web layer issues no token for this route, and
/// cross-user isolation does not apply because the page accepts no user or resource identifier and shows only the signed-in user's own data.
/// </remarks>
[ExcludeFromCodeCoverage]
public class PlaylistsIndexViewEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/playlists";

    private readonly LuminaWebFactory _apiFactory;
    private readonly string _clientIp;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistsIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public PlaylistsIndexViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
        _clientIp = $"10.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(0, 256)}.{Random.Shared.Next(1, 254)}";
    }

    [Fact]
    public async Task PlaylistsIndexView_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);

        // Act
        HttpResponseMessage response = await client.GetAsync(WEB_ROUTE);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(_apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task PlaylistsIndexView_WhenCalledByAuthenticatedUser_ShouldBePrivateAndNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("no-store", string.Join(", ", response.Headers.GetValues("Cache-Control")), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.Contains("__RequestVerificationToken", content, StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("TestPass123!", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PlaylistsIndexView_WhenCalledByAuthenticatedUser_ShouldNotProxyAnyPlaylistCall()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        webClient.Client.DefaultRequestHeaders.Add("X-Forwarded-For", _clientIp);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("users/me/playlists", StringComparison.Ordinal));
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
