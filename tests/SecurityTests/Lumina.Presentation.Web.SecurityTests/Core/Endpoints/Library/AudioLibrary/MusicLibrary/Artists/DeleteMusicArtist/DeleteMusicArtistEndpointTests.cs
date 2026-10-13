#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.DeleteMusicArtist;
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

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.DeleteMusicArtist;

/// <summary>
/// Contains security tests for the <see cref="DeleteMusicArtistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicArtistEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicArtistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeleteMusicArtistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Artists; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task DeleteMusicArtist_WithSQLInjectionInRouteId_ShouldNormalizeTheIdAndNeverDeleteAnotherArtist(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterDeleteSuccess($"libraries/{libraryId}/artists/{normalizedId}");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(maliciousId, libraryId.ToString());
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"libraries/{libraryId}/artists/{normalizedId}", _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicArtist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(artistId.ToString(), libraryId.ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicArtist_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    /// <summary>
    /// Builds the delete request for the artist identified by the route, with the owning library passed through the query string.
    /// </summary>
    /// <param name="artistId">The raw route value of the artist Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    /// <returns>The configured delete request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(string artistId, string libraryId)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/library/audio-library/music-library/artists/{Uri.EscapeDataString(artistId)}/api-delete-artist?libraryId={Uri.EscapeDataString(libraryId)}")
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
