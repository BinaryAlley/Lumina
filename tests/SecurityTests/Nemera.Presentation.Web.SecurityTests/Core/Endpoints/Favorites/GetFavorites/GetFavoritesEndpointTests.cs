#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Favorites;
using Nemera.Presentation.Web.Common.Enums.Favorites;
using Nemera.Presentation.Web.Core.Endpoints.Favorites.GetFavorites;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Common;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Favorites;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// Contains security tests for the <see cref="GetFavoritesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFavoritesEndpointTests : IClassFixture<NemeraWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/favorites/api-get-favorites";

    private readonly NemeraWebFactory _apiFactory;
    private readonly PaginatedDtoFixture<FavoriteItemDto> _paginatedDtoFixture = new();
    private readonly FavoriteItemDtoFixture _favoriteItemDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetFavoritesEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetFavorites_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync(WEB_ROUTE);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetFavoritesEndpointCalls());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // Basic SQL injection.
    [InlineData("'; DROP TABLE Favorites--")] // Destructive injection.
    [InlineData("../../admin")] // Path traversal.
    [InlineData("<script>alert(1)</script>")] // Script injection.
    public async Task GetFavorites_WithInjectionPayloadInMediaKind_ShouldRejectItAndNotCallTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"{WEB_ROUTE}?mediaKind={Uri.EscapeDataString(payload)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The media kind is bound to an enum, so a crafted value is rejected before the endpoint executes and can never alter the upstream URL.
        // The model binding failure is returned through the application's JSON error envelope, so the test asserts the rejection and that the
        // value never reaches the API.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(GetFavoritesEndpointCalls());
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Nemera", content, StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("currentPage", "' OR '1'='1")] // Basic SQL injection in the page number.
    [InlineData("currentPage", "1;DROP TABLE Favorites")] // Destructive injection in the page number.
    [InlineData("perPage", "' OR '1'='1")] // Basic SQL injection in the page size.
    [InlineData("perPage", "../../admin")] // Path traversal in the page size.
    public async Task GetFavorites_WithInjectionPayloadInPagination_ShouldRejectItAndNotCallTheApi(string parameterName, string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"{WEB_ROUTE}?{parameterName}={Uri.EscapeDataString(payload)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The page bounds are bound to integers and rebuilt from the typed values, so a crafted value is rejected before the endpoint executes.
        // The model binding failure is returned through the application's JSON error envelope, so the test asserts the rejection and that the
        // value never reaches the API.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(GetFavoritesEndpointCalls());
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Nemera", content, StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetFavorites_WhenCalledByAuthenticatedUser_ShouldReturnPrivateNoStoreResponseWithoutLeakingInternals()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        FavoriteItemDto expectedItem = _favoriteItemDtoFixture.Create(mediaKind: FavoriteMediaKind.MusicTrack);
        _apiFactory.ApiClientStub.RegisterGetResponse(
            "users/me/favorites?currentPage=1&perPage=200",
            _paginatedDtoFixture.Create(data: [expectedItem], currentPage: 1, perPage: 200, count: 1, numberOfPages: 1));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(WEB_ROUTE);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Nemera", content, StringComparison.Ordinal);
        Assert.Equal(["users/me/favorites?currentPage=1&perPage=200"], GetFavoritesEndpointCalls());
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
