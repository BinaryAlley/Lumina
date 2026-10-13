#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteMusicAlbum;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.DeleteMusicAlbum;

/// <summary>
/// Contains integration tests for the <see cref="DeleteMusicAlbumEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteMusicAlbumEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicAlbumEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeleteMusicAlbumEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DeleteMusicAlbum_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardDeleteAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterDeleteSuccess($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(artistId, albumId, libraryId);
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}", _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicAlbum_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(artistId, albumId, libraryId);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}", _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicAlbum_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    /// <summary>
    /// Builds the delete request for the album identified by the route, with the owning library passed through the query string.
    /// </summary>
    /// <param name="artistId">The route Id of the artist the album belongs to.</param>
    /// <param name="albumId">The route Id of the album to delete.</param>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <returns>The configured delete request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(Guid artistId, Guid albumId, Guid libraryId)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-delete-album?libraryId={libraryId}")
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
