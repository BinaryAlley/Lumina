#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.MovePlaylistEntry;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// Contains integration tests for the <see cref="MovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly MovePlaylistEntryRequestFixture _movePlaylistEntryRequestFixture = new();
    private readonly PlaylistEntryMovedDtoFixture _playlistEntryMovedDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public MovePlaylistEntryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardThePositionAndReturnTheMovedEntry()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create(targetPosition: 2);
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries/{playlistEntryId}/move";
        PlaylistEntryMovedDto expectedResult = _playlistEntryMovedDtoFixture.Create(
            playlistId: playlistId,
            entryId: playlistEntryId,
            position: 2);
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, expectedResult);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage moveRequest = CreateMoveRequest(playlistId.ToString(), playlistEntryId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedResult.PlaylistId, data.GetProperty("playlistId").GetGuid());
        Assert.Equal(expectedResult.EntryId, data.GetProperty("entryId").GetGuid());
        Assert.Equal(expectedResult.Position, data.GetProperty("position").GetInt32());
        List<(string Endpoint, object? Data)> moveRequests = GetMoveRequests();
        (string Endpoint, object? Data) moveRequestCall = Assert.Single(moveRequests);
        Assert.Equal(expectedEndpoint, moveRequestCall.Endpoint);
        MovePlaylistEntryRequest forwardedPayload = Assert.IsType<MovePlaylistEntryRequest>(moveRequestCall.Data);
        Assert.Equal(2, forwardedPayload.TargetPosition);
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenApiRejectsThePosition_ShouldReturnAFailureEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries/{playlistEntryId}/move";
        _apiFactory.ApiClientStub.RegisterPutException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Validation", detail: "TargetPositionOutOfRange"),
            HttpStatusCode.UnprocessableEntity,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage moveRequest = CreateMoveRequest(playlistId.ToString(), playlistEntryId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);
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
    public async Task MovePlaylistEntry_WhenTheEntryDoesNotExist_ShouldReturnNotFoundEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries/{playlistEntryId}/move";
        _apiFactory.ApiClientStub.RegisterPutException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound, expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage moveRequest = CreateMoveRequest(playlistId.ToString(), playlistEntryId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NotFound", json.RootElement.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage moveRequest = CreateMoveRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), request, antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetMoveRequests());
    }

    [Fact]
    public async Task MovePlaylistEntry_WhenRouteIdsAreNotGuids_ShouldForwardTheEmptyGuids()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        MovePlaylistEntryRequest request = _movePlaylistEntryRequestFixture.Create();
        string expectedEndpoint = $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}/move";
        _apiFactory.ApiClientStub.RegisterPutResponse(expectedEndpoint, _playlistEntryMovedDtoFixture.Create(playlistId: Guid.Empty, entryId: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage moveRequest = CreateMoveRequest("not-a-guid", "also-not-a-guid", request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(moveRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetMoveRequests().Select(moveRequestCall => moveRequestCall.Endpoint));
        Assert.DoesNotContain("not-a-guid", string.Join(",", GetMoveRequests().Select(moveRequestCall => moveRequestCall.Endpoint)), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the move entry requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream move entry requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetMoveRequests()
    {
        return [.. _apiFactory.ApiClientStub.PutRequests
            .Where(putRequest => putRequest.Endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal)
                && putRequest.Endpoint.EndsWith("/move", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the move entry request for the specified route values.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="playlistEntryId">The value of the playlist entry identifier route segment.</param>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured move entry request.</returns>
    private static HttpRequestMessage CreateMoveRequest(string playlistId, string playlistEntryId, MovePlaylistEntryRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage moveRequest = new(HttpMethod.Put, $"/en-us/playlists/item/{Uri.EscapeDataString(playlistId)}/entries/{Uri.EscapeDataString(playlistEntryId)}/api-move-entry")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        moveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        moveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            moveRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return moveRequest;
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
