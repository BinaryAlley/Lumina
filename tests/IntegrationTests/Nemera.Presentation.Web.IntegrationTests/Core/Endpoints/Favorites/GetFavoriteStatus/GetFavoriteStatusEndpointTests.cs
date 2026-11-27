#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavoriteStatus;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// Contains integration tests for the <see cref="GetFavoriteStatusEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoriteStatusEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/favorites/api-get-favorite-status";

    private readonly LuminaWebFactory _apiFactory;
    private readonly FavoriteStatusDtoFixture _favoriteStatusDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetFavoriteStatusEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenCalledWithMediaKindAndIds_ShouldForwardThemAndSurfaceTheFavoritedIds()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        List<Guid> mediaItemIds = [Guid.NewGuid(), Guid.NewGuid()];
        string expectedUpstreamEndpoint = $"users/me/favorites/status?mediaKind=MusicTrack&mediaItemIds={mediaItemIds[0]}&mediaItemIds={mediaItemIds[1]}";
        FavoriteStatusDto expectedStatus = _favoriteStatusDtoFixture.Create(favoritedMediaItemIds: [mediaItemIds[1]]);
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedUpstreamEndpoint, expectedStatus);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(
            $"{WEB_ROUTE}?mediaKind=MusicTrack&mediaItemIds={mediaItemIds[0]}&mediaItemIds={mediaItemIds[1]}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        List<Guid> favoritedIds = [.. json.RootElement.GetProperty("data").GetProperty("favoritedMediaItemIds").EnumerateArray().Select(element => element.GetGuid())];
        Assert.Equal(expectedStatus.FavoritedMediaItemIds, favoritedIds);
        Assert.Equal([expectedUpstreamEndpoint], GetFavoriteStatusEndpointCalls());
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenMediaKindAndIdsAreOmitted_ShouldForwardTheBareStatusRoute()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string EXPECTED_UPSTREAM_ENDPOINT = "users/me/favorites/status";
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_ENDPOINT, _favoriteStatusDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([EXPECTED_UPSTREAM_ENDPOINT], GetFavoriteStatusEndpointCalls());
    }

    /// <summary>
    /// Gets the favorite status endpoints that were called upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream favorite status endpoints that were called.</returns>
    private List<string> GetFavoriteStatusEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.GetEndpointsCalled
            .Where(endpoint => endpoint.StartsWith("users/me/favorites/status", StringComparison.Ordinal))];
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
