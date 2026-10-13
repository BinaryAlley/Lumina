#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;

/// <summary>
/// Contains security tests for the <see cref="SaveMusicAlbumEndpoint"/> class.
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

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveMusicAlbum_WithSQLInjectionInRouteId_ShouldNormalizeTheIdAndNeverCallAnotherAlbum(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}", _musicAlbumDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(maliciousId, maliciousId, libraryId.ToString(), _updateMusicAlbumRequestFixture.Create());
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route is authoritative and its values are normalized to Guids before they are substituted into the upstream URL, so a crafted
        // route value can never escape its URL segment or redirect the update to another album, and the response must not leak stack traces
        // or database details.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}");
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveMusicAlbum_WithSQLInjectionInLibraryQueryId_ShouldNormalizeTheLibraryId(string maliciousLibraryId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{normalizedId}/artists/{artistId}/albums/{albumId}", _musicAlbumDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId.ToString(), albumId.ToString(), maliciousLibraryId, _updateMusicAlbumRequestFixture.Create());
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{normalizedId}/artists/{artistId}/albums/{albumId}");
    }

    [Theory]
    [InlineData("'; DROP TABLE Albums; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveMusicAlbum_WithSQLInjectionInPostedTitle_ShouldNotLeakOrExecuteIt(string maliciousTitle)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        UpdateMusicAlbumRequest request = _updateMusicAlbumRequestFixture.Create();
        request.Metadata!.Title = maliciousTitle;
        MusicAlbumDto expectedAlbum = _musicAlbumDtoFixture.Create(id: albumId);
        expectedAlbum.Metadata!.Title = maliciousTitle;
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}", expectedAlbum);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId.ToString(), albumId.ToString(), libraryId.ToString(), request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{artistId}/albums/{albumId}" && putRequest.Data is UpdateMusicAlbumRequest forwarded && forwarded.Metadata!.Title == maliciousTitle);
    }

    [Fact]
    public async Task SaveMusicAlbum_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId.ToString(), albumId.ToString(), libraryId.ToString(), _updateMusicAlbumRequestFixture.Create());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task SaveMusicAlbum_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), _updateMusicAlbumRequestFixture.Create());

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
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    /// <param name="request">The album data to send.</param>
    /// <returns>The configured save request.</returns>
    private static HttpRequestMessage CreateSaveRequest(string artistId, string albumId, string libraryId, UpdateMusicAlbumRequest request)
    {
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(artistId)}/albums/{Uri.EscapeDataString(albumId)}/api-save-album?libraryId={Uri.EscapeDataString(libraryId)}")
        {
            Content = JsonContent.Create(request)
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return saveRequest;
    }
}
