#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.Item;
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
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.Item;

/// <summary>
/// Contains integration tests for the <see cref="PlaylistItemViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistItemViewEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PaginatedDtoFixture<PlaylistEntryDto> _paginatedDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistItemViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public PlaylistItemViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task PlaylistItemView_WhenCalledByAuthenticatedUser_ShouldForwardTheDocumentedUpstreamUrlAndRenderThePlaylist()
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
                numberOfPages: 1));
        string expectedEndpoint = $"users/me/playlists/{playlistId}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedPlaylist);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/item/{playlistId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("playlist-details-form", content, StringComparison.Ordinal);
        Assert.Contains(expectedPlaylist.Name, content, StringComparison.Ordinal);
        Assert.Contains(expectedPlaylist.Description!, content, StringComparison.Ordinal);
        Assert.Contains(expectedEntry.MediaItemTitle, content, StringComparison.Ordinal);
        Assert.Equal([expectedEndpoint], GetPlaylistEndpointCalls());
    }

    [Fact]
    public async Task PlaylistItemView_WhenCalledByAuthenticatedUser_ShouldReturnANonStorablePage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _playlistDtoFixture.Create(id: playlistId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/playlists/item/{playlistId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Contains("no-store", string.Join(", ", response.Headers.GetValues("Cache-Control")), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Cookie", response.Headers.Vary);
    }

    [Fact]
    public async Task PlaylistItemView_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/playlists/{ZERO_GUID}?currentPage=1&perPage=200";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _playlistDtoFixture.Create(id: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/playlists/item/not-a-guid");

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
