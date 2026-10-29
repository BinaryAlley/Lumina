#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Playlists.Item;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.Item;

/// <summary>
/// Contains integration tests for the <see cref="AddPlaylistViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistViewEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/playlists/item";

    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddPlaylistViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AddPlaylistView_WhenCalledByAuthenticatedUser_ShouldRenderTheCreatePlaylistPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("playlist-details-form", content, StringComparison.Ordinal);
        Assert.Contains("playlist-title", content, StringComparison.Ordinal);
        Assert.Contains("Create playlist", content, StringComparison.Ordinal);
        Assert.Contains("__RequestVerificationToken", content, StringComparison.Ordinal);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("users/me/playlists", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AddPlaylistView_WhenCalledByAuthenticatedUser_ShouldReturnANonStorablePage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("no-store", string.Join(", ", response.Headers.GetValues("Cache-Control")), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cookie", response.Headers.Vary);
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
