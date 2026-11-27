#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.AddFavorite;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Favorites;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Favorites.AddFavorite;

/// <summary>
/// Contains integration tests for the <see cref="AddFavoriteEndpoint"/> class.
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
    public async Task AddFavorite_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheFavoriteAndReturnIt()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        FavoriteDto expectedFavorite = _favoriteDtoFixture.Create(
            mediaKind: FavoriteMediaKind.MusicTrack,
            mediaItemId: mediaItemId);
        _apiFactory.ApiClientStub.RegisterPostResponse(UPSTREAM_ENDPOINT, expectedFavorite);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(FavoriteMediaKind.MusicTrack, mediaItemId, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedFavorite.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(nameof(FavoriteMediaKind.MusicTrack), data.GetProperty("mediaKind").GetString());
        Assert.Equal(mediaItemId, data.GetProperty("mediaItemId").GetGuid());
        Assert.Equal(expectedFavorite.FavoritedOnUtc, data.GetProperty("favoritedOnUtc").GetDateTime());
        List<(string Endpoint, object? Data)> addRequests = GetAddRequests();
        (string Endpoint, object? Data) addRequestCall = Assert.Single(addRequests);
        Assert.Equal(UPSTREAM_ENDPOINT, addRequestCall.Endpoint);
        AddFavoriteRequest forwardedPayload = Assert.IsType<AddFavoriteRequest>(addRequestCall.Data);
        Assert.Equal(FavoriteMediaKind.MusicTrack, forwardedPayload.MediaKind);
        Assert.Equal(mediaItemId, forwardedPayload.MediaItemId);
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
    /// Builds the add favorite request for the specified media item.
    /// </summary>
    /// <param name="mediaKind">The kind of the media item to favorite.</param>
    /// <param name="mediaItemId">The Id of the media item to favorite.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured add favorite request.</returns>
    private static HttpRequestMessage CreateAddRequest(FavoriteMediaKind mediaKind, Guid mediaItemId, string antiforgeryToken)
    {
        string payload = $"{{\"mediaKind\":\"{mediaKind}\",\"mediaItemId\":\"{mediaItemId}\"}}";
        HttpRequestMessage addRequest = new(HttpMethod.Post, WEB_ROUTE)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        addRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        addRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
