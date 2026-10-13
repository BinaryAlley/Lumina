#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.SaveMusicArtist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.SaveMusicArtist;

/// <summary>
/// Contains security tests for the <see cref="SaveMusicArtistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveMusicArtistEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly UpdateMusicArtistRequestFixture _updateMusicArtistRequestFixture = new();
    private readonly MusicArtistDtoFixture _musicArtistDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicArtistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SaveMusicArtistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveMusicArtist_WithSQLInjectionInRouteId_ShouldNormalizeTheIdAndNeverCallAnotherArtist(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{normalizedId}", _musicArtistDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(maliciousId, libraryId.ToString(), _updateMusicArtistRequestFixture.Create());
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route is authoritative and its value is normalized to a Guid before it is substituted into the upstream URL, so a crafted
        // route value can never escape its URL segment or redirect the update to another artist, and the response must not leak stack traces
        // or database details.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{normalizedId}");
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveMusicArtist_WithSQLInjectionInLibraryQueryId_ShouldNormalizeTheLibraryId(string maliciousLibraryId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid artistId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{normalizedId}/artists/{artistId}", _musicArtistDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId.ToString(), maliciousLibraryId, _updateMusicArtistRequestFixture.Create());
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{normalizedId}/artists/{artistId}");
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveMusicArtist_WithSQLInjectionInPostedName_ShouldNotLeakOrExecuteIt(string maliciousName)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        UpdateMusicArtistRequest request = _updateMusicArtistRequestFixture.Create();
        request.Metadata!.Name = maliciousName;
        MusicArtistDto expectedArtist = _musicArtistDtoFixture.Create(id: artistId);
        expectedArtist.Metadata!.Name = maliciousName;
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{artistId}", expectedArtist);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId.ToString(), libraryId.ToString(), request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{artistId}" && putRequest.Data is UpdateMusicArtistRequest forwarded && forwarded.Metadata!.Name == maliciousName);
    }

    [Fact]
    public async Task SaveMusicArtist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(artistId.ToString(), libraryId.ToString(), _updateMusicArtistRequestFixture.Create());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task SaveMusicArtist_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), _updateMusicArtistRequestFixture.Create());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Builds the save request that PUTs the given <paramref name="request"/> to the artist identified by the route,
    /// with the owning library passed through the query string.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    /// <param name="request">The artist data to send.</param>
    /// <returns>The configured save request.</returns>
    private static HttpRequestMessage CreateSaveRequest(string artistId, string libraryId, UpdateMusicArtistRequest request)
    {
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(artistId)}/api-save-artist?libraryId={Uri.EscapeDataString(libraryId)}")
        {
            Content = JsonContent.Create(request)
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return saveRequest;
    }
}
