#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetArtistAlbums;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetArtistAlbums;

/// <summary>
/// Contains integration tests for the <see cref="GetArtistAlbumsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PaginatedMusicAlbumLiteDtoFixture _paginatedMusicAlbumLiteDtoFixture = new();
    private readonly MusicAlbumLiteDtoFixture _musicAlbumLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetArtistAlbumsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetArtistAlbums_WhenCalledByAuthenticatedUser_ShouldReturnAlbumsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        PaginatedMusicAlbumLiteDto expectedResponse = _paginatedMusicAlbumLiteDtoFixture.Create(data: [_musicAlbumLiteDtoFixture.Create(title: "Test Album")]);
        string expectedEndpoint = $"libraries/{libraryId}/artists/{artistId}/albums/lite?currentPage=1&perPage=20";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedResponse);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/api-get-albums?libraryId={libraryId}&currentPage=1&perPage=20");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedResponse.Data.Count, json.RootElement.GetProperty("data").GetProperty("data").GetArrayLength());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetArtistAlbums_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/api-get-albums?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }
}
