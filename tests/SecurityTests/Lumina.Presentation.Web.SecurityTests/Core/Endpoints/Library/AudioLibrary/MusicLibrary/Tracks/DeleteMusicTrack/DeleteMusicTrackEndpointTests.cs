#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;

/// <summary>
/// Contains security tests for the <see cref="DeleteMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicTrackEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeleteMusicTrackEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task DeleteMusicTrack_WithSQLInjectionInRouteId_ShouldNormalizeTheIdAndNeverDeleteAnotherTrack(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterDeleteSuccess($"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(maliciousId, maliciousId, maliciousId, libraryId.ToString());
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"libraries/{libraryId}/artists/{normalizedId}/albums/{normalizedId}/tracks/{normalizedId}", _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Theory]
    [InlineData("'; DROP TABLE Tracks; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task DeleteMusicTrack_WithSQLInjectionInLibraryQueryId_ShouldNormalizeTheLibraryId(string maliciousLibraryId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterDeleteSuccess($"libraries/{normalizedId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), maliciousLibraryId);
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"libraries/{normalizedId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicTrack_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(artistId.ToString(), albumId.ToString(), trackId.ToString(), libraryId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicTrack_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    /// <summary>
    /// Builds the delete request for the track identified by the route, with the owning library passed through the query string.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="albumId">The raw route value of the album Id.</param>
    /// <param name="trackId">The raw route value of the track Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    /// <returns>The configured delete request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(string artistId, string albumId, string trackId, string libraryId)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(artistId)}/albums/{Uri.EscapeDataString(albumId)}/tracks/{Uri.EscapeDataString(trackId)}/api-delete-track?libraryId={Uri.EscapeDataString(libraryId)}")
        {
            // The front-end sends the JSON content type even for DELETE requests, which triggers the antiforgery validation.
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        deleteRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return deleteRequest;
    }
}
