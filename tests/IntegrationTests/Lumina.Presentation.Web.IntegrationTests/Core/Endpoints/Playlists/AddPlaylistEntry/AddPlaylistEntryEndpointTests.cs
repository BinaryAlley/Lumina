#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Enums.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntry;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// Contains integration tests for the <see cref="AddPlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntryEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly AddPlaylistEntryRequestFixture _addPlaylistEntryRequestFixture = new();
    private readonly PlaylistEntryDtoFixture _playlistEntryDtoFixture = new();
    private readonly PlaylistEntryAddedDtoFixture _playlistEntryAddedDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddPlaylistEntryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheTrackAndReturnTheAddedEntry()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid trackId = Guid.NewGuid();
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create(trackId: trackId);
        PlaylistEntryDto expectedEntry = _playlistEntryDtoFixture.Create(
            mediaItemId: trackId,
            mediaItemType: PlaylistMediaItemType.MusicTrack,
            position: 3,
            mediaItemTitle: "Added track");
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries";
        PlaylistEntryAddedDto expectedResult = _playlistEntryAddedDtoFixture.Create(playlistId: playlistId, entry: expectedEntry);
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, expectedResult);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(playlistId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(expectedResult.PlaylistId, data.GetProperty("playlistId").GetGuid());
        JsonElement entry = data.GetProperty("entry");
        Assert.Equal(expectedEntry.Id, entry.GetProperty("id").GetGuid());
        Assert.Equal(expectedEntry.MediaItemId, entry.GetProperty("mediaItemId").GetGuid());
        Assert.Equal(nameof(PlaylistMediaItemType.MusicTrack), entry.GetProperty("mediaItemType").GetString());
        Assert.Equal(expectedEntry.Position, entry.GetProperty("position").GetInt32());
        Assert.Equal(expectedEntry.MediaItemTitle, entry.GetProperty("mediaItemTitle").GetString());
        List<(string Endpoint, object? Data)> addRequests = GetAddRequests();
        (string Endpoint, object? Data) addRequestCall = Assert.Single(addRequests);
        Assert.Equal(expectedEndpoint, addRequestCall.Endpoint);
        AddPlaylistEntryRequest forwardedPayload = Assert.IsType<AddPlaylistEntryRequest>(addRequestCall.Data);
        Assert.Equal(trackId, forwardedPayload.TrackId);
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenApiRejectsTheTrack_ShouldReturnAFailureEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries";
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Validation", detail: "MusicTrackNotFound"),
            HttpStatusCode.UnprocessableEntity,
            expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(playlistId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
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
    public async Task AddPlaylistEntry_WhenPlaylistDoesNotExist_ShouldReturnNotFoundEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries";
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound, expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(playlistId.ToString(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NotFound", json.RootElement.GetProperty("errorMessage").GetString());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(playlistId.ToString(), request, antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetAddRequests());
    }

    [Fact]
    public async Task AddPlaylistEntry_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries";
        AddPlaylistEntryRequest request = _addPlaylistEntryRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _playlistEntryAddedDtoFixture.Create(playlistId: playlistId));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(playlistId.ToString().ToUpperInvariant(), request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetAddRequests().Select(addRequestCall => addRequestCall.Endpoint));
        Assert.DoesNotContain(playlistId.ToString().ToUpperInvariant(), string.Join(",", GetAddRequests().Select(addRequestCall => addRequestCall.Endpoint)), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the add entry requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream add entry requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetAddRequests()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal)
                && postRequest.Endpoint.EndsWith("/entries", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the add entry request for the specified playlist route value.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured add entry request.</returns>
    private static HttpRequestMessage CreateAddRequest(string playlistId, AddPlaylistEntryRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage addRequest = new(HttpMethod.Post, $"/en-us/playlists/item/{Uri.EscapeDataString(playlistId)}/entries/api-add-entry")
        {
            Content = JsonContent.Create(request)
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        addRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        addRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            addRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return addRequest;
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
