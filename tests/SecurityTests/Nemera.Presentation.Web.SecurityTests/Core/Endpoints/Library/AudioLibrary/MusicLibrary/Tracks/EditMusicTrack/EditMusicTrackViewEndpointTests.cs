#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.EditMusicTrack;
using Nemera.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.EditMusicTrack;

/// <summary>
/// Contains security tests for the <see cref="EditMusicTrackViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditMusicTrackViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly MusicTrackDtoFixture _musicTrackDtoFixture = new();
    private readonly MusicAlbumDtoFixture _musicAlbumDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditMusicTrackViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditMusicTrackViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditMusicTrack_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/en-us/library/audio-library/music-library/artists/{Guid.NewGuid()}/albums/{Guid.NewGuid()}/tracks/{Guid.NewGuid()}?libraryId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    public async Task EditMusicTrack_WithSQLInjectionInRouteId_ShouldNormalizeTheIdWithoutLeakingData(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        MusicTrackDto expectedTrack = _musicTrackDtoFixture.Create();
        expectedTrack.Metadata!.Title = "A track";
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}", expectedTrack);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}", expectedAlbum);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(maliciousId)}/albums/{Uri.EscapeDataString(maliciousId)}/tracks/{Uri.EscapeDataString(maliciousId)}?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Nemera", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == $"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}");
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == $"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}");
    }
}
