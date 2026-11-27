#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreMusicTrack;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreMusicTrack;

/// <summary>
/// Contains integration tests for the <see cref="RestoreMusicTrackEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreMusicTrackEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreMusicTrackEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RestoreMusicTrackEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RestoreMusicTrack_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardPostAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPostSuccess($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest(artistId, albumId, trackId, libraryId);
        restoreRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("isRestored").GetBoolean());
        Assert.Contains($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore", _apiFactory.ApiClientStub.PostRequests.Select(postRequest => postRequest.Endpoint));
    }

    [Fact]
    public async Task RestoreMusicTrack_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        _apiFactory.ApiClientStub.Reset();
        HttpRequestMessage restoreRequest = CreateRestoreRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PostRequests);
    }

    [Fact]
    public async Task RestoreMusicTrack_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PostRequests);
    }

    /// <summary>
    /// Builds the restore request for the track identified by the route, with the owning library passed through the query string.
    /// </summary>
    /// <param name="artistId">The route Id of the artist the track belongs to.</param>
    /// <param name="albumId">The route Id of the album the track belongs to.</param>
    /// <param name="trackId">The route Id of the track to restore.</param>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <returns>The configured restore request.</returns>
    private static HttpRequestMessage CreateRestoreRequest(Guid artistId, Guid albumId, Guid trackId, Guid libraryId)
    {
        HttpRequestMessage restoreRequest = new(HttpMethod.Post, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-restore-track?libraryId={libraryId}")
        {
            // The front-end sends the JSON content type even for bodyless POST requests, which triggers the antiforgery validation.
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        restoreRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        restoreRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return restoreRequest;
    }
}
