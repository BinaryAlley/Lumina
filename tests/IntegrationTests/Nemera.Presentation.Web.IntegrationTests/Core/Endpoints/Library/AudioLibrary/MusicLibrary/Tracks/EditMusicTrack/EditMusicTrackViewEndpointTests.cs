#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.EditMusicTrack;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.EditMusicTrack;

/// <summary>
/// Contains integration tests for the <see cref="EditMusicTrackViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicTrackViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly MusicTrackDtoFixture _musicTrackDtoFixture = new();
    private readonly MusicAlbumDtoFixture _musicAlbumDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicTrackViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditMusicTrackViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditMusicTrack_WhenCalledByAuthenticatedUser_ShouldRenderEditTrackPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create(id: trackId, albumId: albumId, libraryId: libraryId);
        expectedTrack.Metadata!.Title = "A track";
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", expectedTrack);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}", expectedAlbum);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("A track", content);
        Assert.Contains($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.Contains($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task EditMusicTrack_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }
}
