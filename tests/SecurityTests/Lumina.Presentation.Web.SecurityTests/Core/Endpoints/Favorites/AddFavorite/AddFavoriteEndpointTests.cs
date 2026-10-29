#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.AddFavorite;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Contains security tests for the <see cref="AddFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddFavoriteEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string WEB_ROUTE = "/en-us/favorites/api-add-favorite";
    private const string UPSTREAM_ENDPOINT = "users/me/favorites";

    private readonly LuminaWebFactory _apiFactory;
    private readonly FavoriteDtoFixture _favoriteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddFavoriteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddFavoriteEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AddFavorite_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage addRequest = CreateAddRequest(nameof(FavoriteMediaKind.MusicTrack), Guid.NewGuid().ToString(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetAddRequests());
    }

    [Fact]
    public async Task AddFavorite_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(nameof(FavoriteMediaKind.MusicTrack), Guid.NewGuid().ToString(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetAddRequests());
    }

    [Fact]
    public async Task AddFavorite_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(nameof(FavoriteMediaKind.MusicTrack), Guid.NewGuid().ToString(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetAddRequests());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task AddFavorite_WithInjectionPayloadInMediaKind_ShouldRejectItAndNotCallTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(payload, Guid.NewGuid().ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The media kind is bound to an enum, so the crafted value is rejected before the endpoint executes and can never reach the API.
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Empty(GetAddRequests());
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
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
    public async Task AddFavorite_WithInjectionPayloadInMediaItemId_ShouldRejectItAndNotCallTheApi(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(nameof(FavoriteMediaKind.MusicTrack), payload, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The media item Id is bound to a Guid, so the crafted value is rejected before the endpoint executes and can never reach the API.
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Empty(GetAddRequests());
        Assert.DoesNotContain(payload, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddFavorite_WhenCalledByAuthenticatedUserWithValidToken_ShouldNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        FavoriteDto expectedFavorite = _favoriteDtoFixture.Create(mediaKind: FavoriteMediaKind.MusicTrack, mediaItemId: mediaItemId);
        _apiFactory.ApiClientStub.RegisterPostResponse(UPSTREAM_ENDPOINT, expectedFavorite);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(nameof(FavoriteMediaKind.MusicTrack), mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(GetAddRequests());
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the add favorite requests that were sent upstream, excluding the login request performed to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream add favorite requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetAddRequests()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint == UPSTREAM_ENDPOINT)];
    }

    /// <summary>
    /// Builds the add favorite request for the specified values.
    /// </summary>
    /// <param name="mediaKind">The value of the media kind body property.</param>
    /// <param name="mediaItemId">The value of the media item identifier body property.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured add favorite request.</returns>
    private static HttpRequestMessage CreateAddRequest(string mediaKind, string mediaItemId, string? antiforgeryToken)
    {
        string payload = JsonSerializer.Serialize(new { mediaKind, mediaItemId });
        HttpRequestMessage addRequest = new(HttpMethod.Post, WEB_ROUTE)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        addRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        addRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            addRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return addRequest;
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
