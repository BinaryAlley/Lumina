#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbumCover;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.UpdateAlbumCover;

/// <summary>
/// Contains integration tests for the <see cref="UpdateAlbumCoverEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateAlbumCoverEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly UpdateMusicCoverDtoFixture _updateMusicCoverDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAlbumCoverEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdateAlbumCoverEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldUploadCoverAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        const string EXPECTED_PATH = "/media/albums/cover.png";
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover", _updateMusicCoverDtoFixture.Create(coverPath: EXPECTED_PATH));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(artistId, albumId, libraryId, "cover.png");
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(EXPECTED_PATH, json.RootElement.GetProperty("data").GetString());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover" && putRequest.Data as string == "cover.png");
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(artistId, albumId, libraryId, "cover.png");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover");
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenCalledWithoutUploadedFile_ShouldForwardRequestAndReturnFailure()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid artistId = Guid.NewGuid();
        Guid albumId = Guid.NewGuid();
        ProblemDetailsDto problemDetails = _problemDetailsDtoFixture.Create(title: "General.Validation", detail: "AlbumCoverCannotBeNull");
        _apiFactory.ApiClientStub.RegisterPutException($"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover", new ApiException(problemDetails, HttpStatusCode.UnprocessableEntity, $"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        MultipartFormDataContent noFileForm = [];
        noFileForm.Add(new StringContent("unrelated-field"));
        HttpRequestMessage uploadRequest = new(HttpMethod.Put, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-update-cover?libraryId={libraryId}")
        {
            Content = noFileForm
        };
        uploadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        uploadRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(uploadRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The web does not short-circuit the missing file; it forwards the request, and the API reports the missing cover
        // with the same AlbumCoverCannotBeNull validation error any other client receives.
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover" && putRequest.Data == null);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task UpdateAlbumCover_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage uploadRequest = CreateUploadRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "cover.png");

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(uploadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Builds the multipart PUT request that uploads a cover to the album identified by the route.
    /// </summary>
    /// <param name="artistId">The route Id of the artist the album belongs to.</param>
    /// <param name="albumId">The route Id of the album whose cover is uploaded.</param>
    /// <param name="libraryId">The Id of the media library the album belongs to.</param>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>The configured upload request.</returns>
    private static HttpRequestMessage CreateUploadRequest(Guid artistId, Guid albumId, Guid libraryId, string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes("fake cover payload"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "cover", fileName);
        return new HttpRequestMessage(HttpMethod.Put, $"/en-us/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-update-cover?libraryId={libraryId}")
        {
            Content = form
        };
    }
}
