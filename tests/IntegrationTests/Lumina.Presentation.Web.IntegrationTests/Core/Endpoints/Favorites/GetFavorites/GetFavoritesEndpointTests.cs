#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavorites;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Contains integration tests for the <see cref="GetFavoritesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/favorites/api-get-favorites";

    private readonly LuminaWebFactory _apiFactory;
    private readonly PaginatedDtoFixture<FavoriteItemDto> _paginatedDtoFixture = new();
    private readonly FavoriteItemDtoFixture _favoriteItemDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetFavoritesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetFavorites_WhenCalledByAuthenticatedUser_ShouldSurfaceThePaginatedFavoritesUnchanged()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        FavoriteItemDto expectedItem = _favoriteItemDtoFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            isAvailable: true,
            title: "Time",
            collectionTitle: "The Dark Side of the Moon",
            creatorName: "Pink Floyd",
            coverPath: "covers/dark-side-of-the-moon.jpg");
        PaginatedDto<FavoriteItemDto> expectedPage = _paginatedDtoFixture.Create(
            data: [expectedItem],
            currentPage: 1,
            perPage: 200,
            count: 1,
            numberOfPages: 1);
        _apiFactory.ApiClientStub.RegisterGetResponse("users/me/favorites?currentPage=1&perPage=200", expectedPage);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("currentPage").GetInt32());
        Assert.Equal(200, data.GetProperty("perPage").GetInt32());
        Assert.Equal(1, data.GetProperty("count").GetInt32());
        Assert.Equal(1, data.GetProperty("numberOfPages").GetInt32());
        JsonElement item = Assert.Single(data.GetProperty("data").EnumerateArray());
        Assert.Equal(nameof(FavoriteMediaKind.MusicTrack), item.GetProperty("mediaKind").GetString());
        Assert.Equal(expectedItem.MediaItemId, item.GetProperty("mediaItemId").GetGuid());
        Assert.True(item.GetProperty("isAvailable").GetBoolean());
        Assert.Equal(expectedItem.LibraryId, item.GetProperty("libraryId").GetGuid());
        Assert.Equal(expectedItem.Title, item.GetProperty("title").GetString());
        Assert.Equal(expectedItem.CollectionId, item.GetProperty("collectionId").GetGuid());
        Assert.Equal(expectedItem.CollectionTitle, item.GetProperty("collectionTitle").GetString());
        Assert.Equal(expectedItem.CreatorId, item.GetProperty("creatorId").GetGuid());
        Assert.Equal(expectedItem.CreatorName, item.GetProperty("creatorName").GetString());
        Assert.Equal(expectedItem.CoverPath, item.GetProperty("coverPath").GetString());
        Assert.Equal(["users/me/favorites?currentPage=1&perPage=200"], GetFavoritesEndpointCalls());
    }

    [Theory]
    [InlineData("", "users/me/favorites?currentPage=1&perPage=200")] // neither the page nor the size is supplied
    [InlineData("?currentPage=3", "users/me/favorites?currentPage=3&perPage=200")] // only the page is supplied
    [InlineData("?perPage=25", "users/me/favorites?currentPage=1&perPage=25")] // only the size is supplied
    [InlineData("?currentPage=2&perPage=50", "users/me/favorites?currentPage=2&perPage=50")] // both the page and the size are supplied
    public async Task GetFavorites_WhenCalledWithPagination_ShouldForwardTheDocumentedUpstreamUrl(string queryString, string expectedUpstreamEndpoint)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedUpstreamEndpoint, _paginatedDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"{WEB_ROUTE}{queryString}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedUpstreamEndpoint], GetFavoritesEndpointCalls());
    }

    [Fact]
    public async Task GetFavorites_WhenMediaKindIsSupplied_ShouldAppendItToTheUpstreamUrl()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        const string EXPECTED_UPSTREAM_ENDPOINT = "users/me/favorites?currentPage=1&perPage=200&mediaKind=MusicTrack";
        _apiFactory.ApiClientStub.RegisterGetResponse(EXPECTED_UPSTREAM_ENDPOINT, _paginatedDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"{WEB_ROUTE}?mediaKind=MusicTrack");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([EXPECTED_UPSTREAM_ENDPOINT], GetFavoritesEndpointCalls());
    }

    [Fact]
    public async Task GetFavorites_WhenMediaItemIsUnavailable_ShouldPassTheEmptyDisplayDataThroughUnchanged()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        FavoriteItemDto unavailableItem = _favoriteItemDtoFixture.Create(
            mediaKind: FavoriteMediaKind.Book,
            isAvailable: false,
            libraryId: Guid.Empty,
            title: string.Empty,
            collectionId: Guid.Empty,
            collectionTitle: string.Empty,
            creatorId: Guid.Empty,
            creatorName: string.Empty,
            includeCoverPath: false);
        _apiFactory.ApiClientStub.RegisterGetResponse(
            "users/me/favorites?currentPage=1&perPage=200",
            _paginatedDtoFixture.Create(data: [unavailableItem], currentPage: 1, perPage: 200, count: 1, numberOfPages: 1));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement item = Assert.Single(json.RootElement.GetProperty("data").GetProperty("data").EnumerateArray());
        Assert.False(item.GetProperty("isAvailable").GetBoolean());
        Assert.Equal(Guid.Empty, item.GetProperty("libraryId").GetGuid());
        Assert.Equal(string.Empty, item.GetProperty("title").GetString());
        Assert.Equal(Guid.Empty, item.GetProperty("collectionId").GetGuid());
        Assert.Equal(string.Empty, item.GetProperty("collectionTitle").GetString());
        Assert.Equal(Guid.Empty, item.GetProperty("creatorId").GetGuid());
        Assert.Equal(string.Empty, item.GetProperty("creatorName").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("coverPath").ValueKind);
    }

    /// <summary>
    /// Gets the favorites endpoints that were called upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream favorites endpoints that were called.</returns>
    private List<string> GetFavoritesEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.GetEndpointsCalled
            .Where(endpoint => endpoint.StartsWith("users/me/favorites", StringComparison.Ordinal))];
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
