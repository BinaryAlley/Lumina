#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.EditMusicAlbum;
using Nemera.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.EditMusicAlbum;

/// <summary>
/// Contains integration tests for the <see cref="EditMusicAlbumViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicAlbumViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly MusicAlbumDtoFixture _musicAlbumDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicAlbumViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditMusicAlbumViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditMusicAlbum_WhenCalledByAuthenticatedUser_ShouldRenderEditAlbumPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create();
        expectedAlbum.Metadata!.Title = "An album";
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{expectedAlbum.LibraryId}/artists/{expectedAlbum.ArtistId}/albums/{expectedAlbum.Id}", expectedAlbum);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{expectedAlbum.ArtistId}/albums/{expectedAlbum.Id}?libraryId={expectedAlbum.LibraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("An album", content);
    }
}
