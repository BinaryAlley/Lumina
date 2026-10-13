#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.UpdateArtistCover;

/// <summary>
/// Contains security tests for the <see cref="UpdateArtistCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistCoverEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly UpdateMusicCoverDtoFixture _updateMusicCoverDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateArtistCoverEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdateArtistCoverEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateArtistCover_WithSQLInjectionInRouteId_ShouldNeverReachAnArtistOrLeakData(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{Guid.Empty}/cover", _updateMusicCoverDtoFixture.Create(coverPath: "/media/artists/cover.png"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(maliciousId, libraryId.ToString(), "cover.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route is authoritative: an unparseable route value becomes an empty artist Id, so the upload can never reach a real artist.
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{Guid.Empty}/cover");
    }

    [Theory]
    [InlineData("'; DROP TABLE MusicArtwork; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task UpdateArtistCover_WithSQLInjectionInLibraryQueryId_ShouldNormalizeItAndNotLeakData(string maliciousLibraryId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid artistId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{Guid.Empty}/artists/{artistId}/cover", _updateMusicCoverDtoFixture.Create(coverPath: "/media/artists/cover.png"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(artistId.ToString(), maliciousLibraryId, "cover.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The library id is normalized to a Guid before it is substituted into the upstream URL, so a crafted value can never escape its URL segment.
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{Guid.Empty}/artists/{artistId}/cover");
    }

    [Fact]
    public async Task UpdateArtistCover_WithPathTraversalFileName_ShouldNotLeakData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{artistId}/cover", _updateMusicCoverDtoFixture.Create(coverPath: "/media/artists/cover.png"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(artistId.ToString(), libraryId.ToString(), "..\\..\\evil.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("..\\", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateArtistCover_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(artistId.ToString(), libraryId.ToString(), "cover.png");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task UpdateArtistCover_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), "cover.png");

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Builds the multipart PUT request that uploads a cover to the artist identified by the route.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>The configured upload request.</returns>
    private static HttpRequestMessage CreateUploadRequest(string artistId, string libraryId, string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes("fake cover payload"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return new HttpRequestMessage(HttpMethod.Put, $"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(artistId)}/api-update-cover?libraryId={Uri.EscapeDataString(libraryId)}")
        {
            Content = form
        };
    }
}
