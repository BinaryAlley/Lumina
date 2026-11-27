#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Web.Common.DTO.Libraries;
using Nemera.Presentation.Web.Common.Exceptions;
using Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.DeletedMusicItems;
using Nemera.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Common;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.DeletedMusicItems;

/// <summary>
/// Contains integration tests for the <see cref="DeletedMusicItemsViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedMusicItemsViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly DeletedMusicArtistDtoFixture _deletedMusicArtistDtoFixture = new();
    private readonly DeletedMusicAlbumDtoFixture _deletedMusicAlbumDtoFixture = new();
    private readonly DeletedMusicTrackDtoFixture _deletedMusicTrackDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedMusicItemsViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeletedMusicItemsViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DeletedMusicItemsView_WhenCalledByAuthenticatedUserWithLibraryId_ShouldRenderDeletedMusicItemsPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        LibraryDto library = _libraryDtoFixture.Create(id: libraryId, title: "Music");
        DeletedMusicArtistDto deletedArtist = _deletedMusicArtistDtoFixture.Create(libraryId: libraryId, id: artistId, name: "Deleted Artist");
        DeletedMusicAlbumDto deletedAlbum = _deletedMusicAlbumDtoFixture.Create(libraryId: libraryId, id: albumId, artistId: artistId, artistName: "Deleted Artist", title: "Deleted Album");
        DeletedMusicTrackDto deletedTrack = _deletedMusicTrackDtoFixture.Create(libraryId: libraryId, artistId: artistId, artistName: "Deleted Artist", albumId: albumId, albumTitle: "Deleted Album", title: "Deleted Track");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", library);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/deleted-items", new[] { deletedArtist });
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/albums/deleted-items", new[] { deletedAlbum });
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/tracks/deleted-items", new[] { deletedTrack });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/deleted-items?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("deleted-artists-table", content);
        Assert.Contains("deleted-albums-table", content);
        Assert.Contains("deleted-tracks-table", content);
        Assert.Contains("btn-restore-artist", content);
        Assert.Contains("btn-restore-album", content);
        Assert.Contains("btn-restore-track", content);
        Assert.Contains("Deleted Artist", content);
        Assert.Contains("Deleted Album", content);
        Assert.Contains("Deleted Track", content);
        Assert.Contains($"libraries/{libraryId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.Contains($"libraries/{libraryId}/artists/deleted-items", _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.Contains($"libraries/{libraryId}/albums/deleted-items", _apiFactory.ApiClientStub.GetEndpointsCalled);
        Assert.Contains($"libraries/{libraryId}/tracks/deleted-items", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task DeletedMusicItemsView_WhenDeletedItemNamesContainMarkup_ShouldEncodeThem()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto library = _libraryDtoFixture.Create(id: libraryId, title: "Music");
        DeletedMusicArtistDto deletedArtist = _deletedMusicArtistDtoFixture.Create(libraryId: libraryId, name: "<script>alert('x')</script>");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", library);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artists/deleted-items", new[] { deletedArtist });
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/albums/deleted-items", Array.Empty<DeletedMusicAlbumDto>());
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/tracks/deleted-items", Array.Empty<DeletedMusicTrackDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/deleted-items?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("&lt;script&gt;alert", content);
        Assert.DoesNotContain("<script>alert('x')</script>", content);
    }

    [Fact]
    public async Task DeletedMusicItemsView_WhenTheApiReturnsForbidden_ShouldRedirectWithoutLeakingDetails()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto library = _libraryDtoFixture.Create(id: libraryId, title: "Music");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", library);
        string endpoint = $"libraries/{libraryId}/artists/deleted-items";
        _apiFactory.ApiClientStub.RegisterGetException(endpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Unauthorized", detail: "NotAuthorized"),
            HttpStatusCode.Forbidden,
            endpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/audio-library/music-library/deleted-items?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // A browser navigation that the API forbids is redirected to the access denied page, without rendering any of the forbidden data.
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeletedMusicItemsView_WhenCalledWithoutLibraryId_ShouldRedirectToHome()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/library/audio-library/music-library/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/en-us", response.Headers.Location!.ToString());
    }
}
