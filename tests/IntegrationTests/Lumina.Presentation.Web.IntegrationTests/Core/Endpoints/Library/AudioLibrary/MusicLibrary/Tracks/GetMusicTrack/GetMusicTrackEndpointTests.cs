#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;

/// <summary>
/// Contains integration tests for the <see cref="GetMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicTrackEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly MusicTrackDtoFixture _musicTrackDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetMusicTrackEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetMusicTrack_WhenCalledByAuthenticatedUser_ShouldReturnTrackFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", expectedTrack);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-track?libraryId={libraryId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(trackId, json.RootElement.GetProperty("data").GetProperty("id").GetGuid());
        Assert.Contains($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetMusicTrack_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}/api-get-track?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }
}
