#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.EditMusicArtist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.EditMusicArtist;

/// <summary>
/// Contains integration tests for the <see cref="EditMusicArtistViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicArtistViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly MusicArtistDtoFixture _musicArtistDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicArtistViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditMusicArtistViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditMusicArtist_WhenCalledByAuthenticatedUser_ShouldRenderEditArtistPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        MusicArtistDto expectedArtist = _musicArtistDtoFixture.Create();
        expectedArtist.Metadata!.Name = "An artist";
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{expectedArtist.LibraryId}/artists/{expectedArtist.Id}", expectedArtist);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{expectedArtist.Id}?libraryId={expectedArtist.LibraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("An artist", content);
    }

    [Fact]
    public async Task EditMusicArtist_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("libraries/", StringComparison.OrdinalIgnoreCase));
    }
}
