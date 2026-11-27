#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylist;
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
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// Contains integration tests for the <see cref="GetPlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PaginatedDtoFixture<PlaylistEntryDto> _paginatedDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPlaylist_WhenCalledByAuthenticatedUser_ShouldForwardTheDocumentedUpstreamUrlAndSurfaceThePlaylist()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        PlaylistEntryDto expectedEntry = _playlistEntryDtoFixture.Create(
            position: 0,
            mediaItemTitle: "First track",
            mediaItemType: PlaylistMediaItemType.MusicTrack);
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create(
            id: playlistId,
            name: "Evening listening",
            description: "Quiet tracks for the end of the day.",
            entries: _paginatedDtoFixture.Create(
                data: [expectedEntry],
                currentPage: 1,
                perPage: 200,
                count: 1,
                numberOfPages: 1),
            createdOnUtc: new DateTime(2026, 10, 19, 18, 0, 0, DateTimeKind.Utc),
            updatedOnUtc: new DateTime(2026, 10, 19, 19, 30, 0, DateTimeKind.Utc));
        string expectedEndpoint = $"users/me/playlists/{playlistId}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedPlaylist);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/item/{playlistId}/api-get-playlist");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType!.MediaType);
        using JsonDocument jsonDocument = JsonDocument.Parse(content);
        Assert.True(jsonDocument.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = jsonDocument.RootElement.GetProperty("data");
        Assert.Equal(expectedPlaylist.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(expectedPlaylist.Name, data.GetProperty("name").GetString());
        Assert.Equal(expectedPlaylist.Description, data.GetProperty("description").GetString());
        Assert.Equal(nameof(PlaylistType.Music), data.GetProperty("type").GetString());
        Assert.Equal(expectedPlaylist.CreatedOnUtc, data.GetProperty("createdOnUtc").GetDateTime());
        Assert.Equal(expectedPlaylist.UpdatedOnUtc, data.GetProperty("updatedOnUtc").GetDateTime());
        JsonElement entries = data.GetProperty("entries");
        Assert.Equal(expectedPlaylist.Entries.CurrentPage, entries.GetProperty("currentPage").GetInt32());
        Assert.Equal(expectedPlaylist.Entries.PerPage, entries.GetProperty("perPage").GetInt32());
        Assert.Equal(expectedPlaylist.Entries.Count, entries.GetProperty("count").GetInt32());
        Assert.Equal(expectedPlaylist.Entries.NumberOfPages, entries.GetProperty("numberOfPages").GetInt32());
        JsonElement entry = Assert.Single(entries.GetProperty("data").EnumerateArray());
        Assert.Equal(expectedEntry.Id, entry.GetProperty("id").GetGuid());
        Assert.Equal(expectedEntry.MediaItemTitle, entry.GetProperty("mediaItemTitle").GetString());
        Assert.Equal([expectedEndpoint], GetPlaylistEndpointCalls());
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task GetPlaylist_WhenPageParametersAreProvided_ShouldForwardThemToTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}?currentPage=2&perPage=50";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _playlistDtoFixture.Create(id: playlistId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/item/{playlistId}/api-get-playlist?currentPage=2&perPage=50");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetPlaylistEndpointCalls());
    }

    [Fact]
    public async Task GetPlaylist_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        string uppercasePlaylistId = playlistId.ToString().ToUpperInvariant();
        string expectedEndpoint = $"users/me/playlists/{playlistId}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _playlistDtoFixture.Create(id: playlistId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/item/{uppercasePlaylistId}/api-get-playlist");

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetPlaylistEndpointCalls();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.DoesNotContain(uppercasePlaylistId, string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPlaylist_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _playlistDtoFixture.Create(id: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/playlists/item/not-a-guid/api-get-playlist");

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> endpointCalls = GetPlaylistEndpointCalls();
        Assert.Equal([expectedEndpoint], endpointCalls);
        Assert.DoesNotContain("not-a-guid", string.Join(",", endpointCalls), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the playlist endpoints that were called upstream, excluding the endpoints called by the authentication helpers.
    /// </summary>
    /// <returns>The list of upstream playlist endpoints that were called.</returns>
    private List<string> GetPlaylistEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.GetEndpointsCalled
            .Where(endpoint => endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal))];
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
