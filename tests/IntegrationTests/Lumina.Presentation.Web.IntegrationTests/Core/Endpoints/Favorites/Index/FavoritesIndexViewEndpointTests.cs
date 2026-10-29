#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Favorites.Index;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Favorites.Index;

/// <summary>
/// Contains integration tests for the <see cref="FavoritesIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FavoritesIndexViewEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/favorites";

    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FavoritesIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public FavoritesIndexViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task FavoritesIndexView_WhenCalledByAuthenticatedUser_ShouldRenderTheFavoritesPage()
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
        Assert.Contains("favorites-container", content, StringComparison.Ordinal);
        Assert.Contains("favorites-kind-filter", content, StringComparison.Ordinal);
        Assert.Contains("no-favorites", content, StringComparison.Ordinal);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("no-store", string.Join(", ", response.Headers.GetValues("Cache-Control")), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task FavoritesIndexView_WhenCalledByAuthenticatedUser_ShouldRenderTheAntiforgeryTokenAndTheLocalizedStrings()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("__RequestVerificationToken", content, StringComparison.Ordinal);
        Assert.Contains("Favorites", content, StringComparison.Ordinal);
        Assert.Contains("You have no favorites yet.", content, StringComparison.Ordinal);
        Assert.Contains("Load more", content, StringComparison.Ordinal);
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
