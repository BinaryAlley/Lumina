#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.CreatePlaylist;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// Contains integration tests for the <see cref="CreatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreatePlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string UPSTREAM_ENDPOINT = "users/me/playlists";

    private readonly LuminaWebFactory _apiFactory;
    private readonly CreatePlaylistRequestFixture _createPlaylistRequestFixture = new();
    private readonly PlaylistDtoFixture _playlistDtoFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PaginatedDtoFixture<PlaylistEntryDto> _paginatedDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CreatePlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CreatePlaylist_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheDetailsAndReturnThePlaylist()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(
            name: "Road trip",
            description: "Songs for the highway.",
            type: PlaylistType.Music);
        PlaylistDto expectedPlaylist = _playlistDtoFixture.Create(
            name: request.Name!,
            description: request.Description,
            type: PlaylistType.Music,
            entries: _paginatedDtoFixture.Create(data: _playlistEntryDtoFixture.CreateMany(1)));
        _apiFactory.ApiClientStub.RegisterPostResponse(UPSTREAM_ENDPOINT, expectedPlaylist);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = CreateCreateRequest(request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedPlaylist.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(expectedPlaylist.Name, data.GetProperty("name").GetString());
        Assert.Equal(expectedPlaylist.Description, data.GetProperty("description").GetString());
        Assert.Equal(nameof(PlaylistType.Music), data.GetProperty("type").GetString());
        List<(string Endpoint, object? Data)> createRequests = GetCreateRequests();
        (string Endpoint, object? Data) createRequestCall = Assert.Single(createRequests);
        Assert.Equal(UPSTREAM_ENDPOINT, createRequestCall.Endpoint);
        CreatePlaylistRequest forwardedPayload = Assert.IsType<CreatePlaylistRequest>(createRequestCall.Data);
        Assert.Equal(request.Name, forwardedPayload.Name);
        Assert.Equal(request.Description, forwardedPayload.Description);
        Assert.Equal(PlaylistType.Music, forwardedPayload.Type);
    }

    [Fact]
    public async Task CreatePlaylist_WhenNoDescriptionIsProvided_ShouldForwardTheNullDescription()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(name: "No description", includeDescription: false);
        _apiFactory.ApiClientStub.RegisterPostResponse(UPSTREAM_ENDPOINT, _playlistDtoFixture.Create(includeDescription: false));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = CreateCreateRequest(request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) createRequestCall = Assert.Single(GetCreateRequests());
        CreatePlaylistRequest forwardedPayload = Assert.IsType<CreatePlaylistRequest>(createRequestCall.Data);
        Assert.Null(forwardedPayload.Description);
    }

    [Fact]
    public async Task CreatePlaylist_WhenApiRejectsTheDetails_ShouldReturnAFailureEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create(includeName: false);
        _apiFactory.ApiClientStub.RegisterPostException(UPSTREAM_ENDPOINT, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Validation", detail: "PlaylistNameCannotBeEmpty"),
            HttpStatusCode.UnprocessableEntity,
            UPSTREAM_ENDPOINT));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = CreateCreateRequest(request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("<b>", json.RootElement.GetProperty("errorMessage").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreatePlaylist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        CreatePlaylistRequest request = _createPlaylistRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage createRequest = CreateCreateRequest(request, antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetCreateRequests());
    }

    /// <summary>
    /// Gets the create playlist requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream create playlist requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetCreateRequests()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint == UPSTREAM_ENDPOINT)];
    }

    /// <summary>
    /// Builds the create playlist request that sends the given <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured create playlist request.</returns>
    private static HttpRequestMessage CreateCreateRequest(CreatePlaylistRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage createRequest = new(HttpMethod.Post, "/en-us/playlists/api-create-playlist")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        createRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            createRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return createRequest;
    }

    /// <summary>
    /// Releases the resources used by the test class and resets the remote API stub.
    /// </summary>
    public void Dispose()
    {
        _apiFactory.ApiClientStub.Reset();
        GC.SuppressFinalize(this);
    }
}
