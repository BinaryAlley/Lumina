#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;

/// <summary>
/// Contains integration tests for the <see cref="DeleteMusicTrackEndpoint"/> class.
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

    [Fact]
    public async Task DeleteMusicTrack_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardDeleteAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterDeleteSuccess($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(artistId, albumId, trackId, libraryId);
        deleteRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicTrack_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(artistId, albumId, trackId, libraryId);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}", _apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    [Fact]
    public async Task DeleteMusicTrack_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.DeleteEndpointsCalled);
    }

    /// <summary>
    /// Builds the delete request for the track identified by the route, with the owning library passed through the query string.
    /// </summary>
    /// <param name="artistId">The route Id of the artist the track belongs to.</param>
    /// <param name="albumId">The route Id of the album the track belongs to.</param>
    /// <param name="trackId">The route Id of the track to delete.</param>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <returns>The configured delete request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(Guid artistId, Guid albumId, Guid trackId, Guid libraryId)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-delete-track?libraryId={libraryId}")
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
