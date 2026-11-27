#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.EditMusicAlbum;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.EditMusicAlbum;

/// <summary>
/// Contains security tests for the <see cref="EditMusicAlbumViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicAlbumViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicAlbumViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditMusicAlbumViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditMusicAlbum_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }
}
