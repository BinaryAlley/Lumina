#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;

/// <summary>
/// Contains integration tests for the <see cref="SaveMusicAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicAlbumEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly UpdateMusicAlbumRequestFixture _updateMusicAlbumRequestFixture = new();
    private readonly MusicAlbumDtoFixture _musicAlbumDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicAlbumEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SaveMusicAlbumEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SaveMusicAlbum_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardUpdateAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create(id: albumId, artistId: artistId, libraryId: libraryId);
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}", expectedAlbum);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId, albumId, libraryId, request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(albumId, json.RootElement.GetProperty("data").GetProperty("id").GetGuid());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{artistId}/albums/{albumId}" && putRequest.Data is UpdateMusicAlbumRequest forwarded && forwarded.Metadata!.Title == request.Metadata!.Title && forwarded.ASIN == request.ASIN);
    }

    [Fact]
    public async Task SaveMusicAlbum_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId, albumId, libraryId, request);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{artistId}/albums/{albumId}");
    }

    [Fact]
    public async Task SaveMusicAlbum_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), _updateMusicAlbumRequestFixture.Create());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Builds the save request that PUTs the given <paramref name="request"/> to the album identified by the route,
    /// with the owning library passed through the query string.
    /// </summary>
    /// <param name="artistId">The route Id of the artist the album belongs to.</param>
    /// <param name="albumId">The route Id of the album to save.</param>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="request">The album data to send.</param>
    /// <returns>The configured save request.</returns>
    private static HttpRequestMessage CreateSaveRequest(Guid artistId, Guid albumId, Guid libraryId, UpdateMusicAlbumRequest request)
    {
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-save-album?libraryId={libraryId}")
        {
            Content = JsonContent.Create(request)
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return saveRequest;
    }
}
