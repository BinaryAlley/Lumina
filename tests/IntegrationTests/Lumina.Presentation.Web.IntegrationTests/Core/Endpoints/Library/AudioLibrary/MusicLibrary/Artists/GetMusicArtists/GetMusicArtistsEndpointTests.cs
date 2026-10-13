#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;

/// <summary>
/// Contains integration tests for the <see cref="GetMusicArtistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PaginatedMusicArtistLiteDtoFixture _paginatedMusicArtistLiteDtoFixture = new();
    private readonly MusicArtistLiteDtoFixture _musicArtistLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetMusicArtistsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetMusicArtists_WhenCalledByAuthenticatedUser_ShouldReturnArtistsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        const string EXPECTED_ENDPOINT = "libraries/{0}/artists/lite?currentPage=1&perPage=20&shouldIgnoreThePrefixForAlphaPicker=False";
        _apiFactory.ApiClientStub.RegisterGetResponse(
            string.Format(EXPECTED_ENDPOINT, libraryId),
            _paginatedMusicArtistLiteDtoFixture.Create(data: [_musicArtistLiteDtoFixture.Create(name: "Test Artist")], currentPage: 1, perPage: 20, count: 1, numberOfPages: 1));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/audio-library/music-library/artists/api-get-artists?libraryId={libraryId}&currentPage=1&perPage=20&shouldIgnoreThePrefixForAlphaPicker=false");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(1, json.RootElement.GetProperty("data").GetProperty("currentPage").GetInt32());
        Assert.Contains(string.Format(EXPECTED_ENDPOINT, libraryId), _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetMusicArtists_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/api-get-artists?libraryId={Guid.NewGuid()}&shouldIgnoreThePrefixForAlphaPicker=false");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }
}
