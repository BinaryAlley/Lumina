#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.UpdatePlaylist;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// Contains integration tests for the <see cref="UpdatePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly UpdatePlaylistRequestFixture _updatePlaylistRequestFixture = new();
    private readonly PlaylistDetailsDtoFixture _playlistDetailsDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdatePlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdatePlaylist_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheDetailsAndReturnThePlaylist()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(
            name: "Renamed playlist",
            description: "Updated description.");
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        PlaylistDetailsDto expectedResult = _playlistDetailsDtoFixture.Create(
            id: playlistId,
            name: request.Name!,
            description: request.Description!,
            type: PlaylistType.Music);
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, expectedResult);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = CreateUpdateRequest(playlistId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedResult.Id, data.GetProperty("id").GetGuid());
        Assert.Equal(expectedResult.Name, data.GetProperty("name").GetString());
        Assert.Equal(expectedResult.Description, data.GetProperty("description").GetString());
        Assert.Equal(nameof(PlaylistType.Music), data.GetProperty("type").GetString());
        Assert.Equal(expectedResult.UpdatedOnUtc, data.GetProperty("updatedOnUtc").GetDateTime());
        List<(string Endpoint, object? Data)> updateRequests = GetUpdateRequests();
        (string Endpoint, object? Data) updateRequestCall = Assert.Single(updateRequests);
        Assert.Equal(expectedEndpoint, updateRequestCall.Endpoint);
        UpdatePlaylistRequest forwardedPayload = Assert.IsType<UpdatePlaylistRequest>(updateRequestCall.Data);
        Assert.Equal(request.Name, forwardedPayload.Name);
        Assert.Equal(request.Description, forwardedPayload.Description);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenNoDescriptionIsProvided_ShouldForwardTheNullDescription()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(name: "Cleared description", includeDescription: false);
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, _playlistDetailsDtoFixture.Create(id: playlistId, includeDescription: false));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = CreateUpdateRequest(playlistId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        (string Endpoint, object? Data) updateRequestCall = Assert.Single(GetUpdateRequests());
        UpdatePlaylistRequest forwardedPayload = Assert.IsType<UpdatePlaylistRequest>(updateRequestCall.Data);
        Assert.Null(forwardedPayload.Description);
    }

    [Fact]
    public async Task UpdatePlaylist_WhenApiRejectsTheDetails_ShouldReturnAFailureEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create(includeName: false);
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterPutException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Validation", detail: "PlaylistNameCannotBeEmpty"),
            HttpStatusCode.UnprocessableEntity,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = CreateUpdateRequest(playlistId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
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
    public async Task UpdatePlaylist_WhenPlaylistDoesNotExist_ShouldReturnNotFoundEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterPutException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound, expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = CreateUpdateRequest(playlistId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NotFound", json.RootElement.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task UpdatePlaylist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = CreateUpdateRequest(playlistId.ToString(), request, antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetUpdateRequests());
    }

    [Fact]
    public async Task UpdatePlaylist_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        UpdatePlaylistRequest request = _updatePlaylistRequestFixture.Create();
        string expectedEndpoint = $"users/me/playlists/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, _playlistDetailsDtoFixture.Create(id: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = CreateUpdateRequest("not-a-guid", request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetUpdateRequests().Select(updateRequestCall => updateRequestCall.Endpoint));
        Assert.DoesNotContain("not-a-guid", string.Join(",", GetUpdateRequests().Select(updateRequestCall => updateRequestCall.Endpoint)), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the update playlist requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream update playlist requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetUpdateRequests()
    {
        return [.. _apiFactory.ApiClientStub.PutRequests
            .Where(putRequest => putRequest.Endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the update playlist request for the specified route value.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured update playlist request.</returns>
    private static HttpRequestMessage CreateUpdateRequest(string playlistId, UpdatePlaylistRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage updateRequest = new(HttpMethod.Put, $"/en-us/playlists/item/{Uri.EscapeDataString(playlistId)}/api-update-playlist")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return updateRequest;
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
