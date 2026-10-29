#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylists;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaylistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistsEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string UPSTREAM_ENDPOINT = "users/me/playlists";

    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaylistSummaryDtoFixture _playlistSummaryDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPlaylistsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPlaylists_WhenCalledByAuthenticatedUser_ShouldReturnThePlaylistsWithNonStorableCacheHeaders()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        PlaylistSummaryDto firstPlaylist = _playlistSummaryDtoFixture.Create(
            name: "Evening listening",
            description: "Quiet tracks.",
            type: PlaylistType.Music,
            entryCount: 7);
        PlaylistSummaryDto secondPlaylist = _playlistSummaryDtoFixture.Create(
            name: "Morning run",
            type: PlaylistType.Music,
            entryCount: 0);
        _apiFactory.ApiClientStub.RegisterGetResponse(UPSTREAM_ENDPOINT, new[] { firstPlaylist, secondPlaylist });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/playlists/api-get-playlists");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        List<JsonElement> data = [.. json.RootElement.GetProperty("data").EnumerateArray()];
        Assert.Equal(2, data.Count);
        Assert.Equal(firstPlaylist.Id, data[0].GetProperty("id").GetGuid());
        Assert.Equal(firstPlaylist.Name, data[0].GetProperty("name").GetString());
        Assert.Equal(firstPlaylist.Description, data[0].GetProperty("description").GetString());
        Assert.Equal(nameof(PlaylistType.Music), data[0].GetProperty("type").GetString());
        Assert.Equal(firstPlaylist.EntryCount, data[0].GetProperty("entryCount").GetInt32());
        Assert.Equal(firstPlaylist.CreatedOnUtc, data[0].GetProperty("createdOnUtc").GetDateTime());
        Assert.Equal(secondPlaylist.Id, data[1].GetProperty("id").GetGuid());
        Assert.Equal([UPSTREAM_ENDPOINT], GetPlaylistEndpointCalls());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task GetPlaylists_WhenUserHasNoPlaylists_ShouldReturnAnEmptyArray()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse(UPSTREAM_ENDPOINT, Array.Empty<PlaylistSummaryDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/playlists/api-get-playlists");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Empty(json.RootElement.GetProperty("data").EnumerateArray());
    }

    [Fact]
    public async Task GetPlaylists_WhenApiReportsAnError_ShouldReturnAFailureEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetException(UPSTREAM_ENDPOINT, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Failure", detail: "UnexpectedError"),
            HttpStatusCode.InternalServerError,
            UPSTREAM_ENDPOINT));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/playlists/api-get-playlists");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPlaylists_WhenApiReportsNotFound_ShouldReturnTheNotFoundEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetException(UPSTREAM_ENDPOINT, new ApiException(null, HttpStatusCode.NotFound, UPSTREAM_ENDPOINT));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/playlists/api-get-playlists");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NotFound", json.RootElement.GetProperty("errorMessage").GetString());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
    }

    /// <summary>
    /// Gets the playlist endpoints that were called upstream, excluding the endpoints called by the authentication helpers.
    /// </summary>
    /// <returns>The list of upstream playlist endpoints that were called.</returns>
    private List<string> GetPlaylistEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.GetEndpointsCalled
            .Where(endpoint => endpoint == UPSTREAM_ENDPOINT)];
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
