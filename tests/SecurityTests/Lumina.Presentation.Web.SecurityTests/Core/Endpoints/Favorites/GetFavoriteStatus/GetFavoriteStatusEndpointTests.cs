#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavoriteStatus;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// Contains security tests for the <see cref="GetFavoriteStatusEndpoint"/> class.
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
    public async Task GetFavoriteStatus_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"{WEB_ROUTE}?mediaKind=MusicTrack&mediaItemIds={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetFavoriteStatusEndpointCalls());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task GetFavoriteStatus_WithInjectionPayloadInMediaKind_ShouldRejectItAndNotCallTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"{WEB_ROUTE}?mediaKind={Uri.EscapeDataString(payload)}&mediaItemIds={Guid.NewGuid()}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The media kind is bound to an enum, so a crafted value is rejected before the endpoint executes and can never alter the upstream URL.
        // The model binding failure is returned through the application's JSON error envelope, so the test asserts the rejection and that the
        // value never reaches the API.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(GetFavoriteStatusEndpointCalls());
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task GetFavoriteStatus_WithInjectionPayloadInMediaItemIds_ShouldRejectItAndNotCallTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"{WEB_ROUTE}?mediaKind=MusicTrack&mediaItemIds={Uri.EscapeDataString(payload)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The media item Ids are bound to Guids, so a crafted value is rejected before the endpoint executes and can never alter the upstream URL.
        // The model binding failure is returned through the application's JSON error envelope, so the test asserts the rejection and that the
        // value never reaches the API.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(GetFavoriteStatusEndpointCalls());
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetFavoriteStatus_WhenCalledByAuthenticatedUser_ShouldReturnPrivateNoStoreResponseWithoutLeakingInternals()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        List<Guid> mediaItemIds = [Guid.NewGuid(), Guid.NewGuid()];
        string expectedUpstreamEndpoint = $"users/me/favorites/status?mediaKind=MusicTrack&mediaItemIds={mediaItemIds[0]}&mediaItemIds={mediaItemIds[1]}";
        FavoriteStatusDto expectedStatus = _favoriteStatusDtoFixture.Create(favoritedMediaItemIds: [mediaItemIds[0]]);
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedUpstreamEndpoint, expectedStatus);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync(
            $"{WEB_ROUTE}?mediaKind=MusicTrack&mediaItemIds={mediaItemIds[0]}&mediaItemIds={mediaItemIds[1]}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.Equal([expectedUpstreamEndpoint], GetFavoriteStatusEndpointCalls());
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
