#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntries;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// Contains integration tests for the <see cref="AddPlaylistEntriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddPlaylistEntriesEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;
    private readonly AddPlaylistEntriesRequestFixture _addPlaylistEntriesRequestFixture = new();
    private readonly PlaylistEntriesAddedDtoFixture _playlistEntriesAddedDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public AddPlaylistEntriesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheEntriesAndReturnTheAddedCount()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        List<Guid> trackIds = [Guid.NewGuid(), Guid.NewGuid()];
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create(trackIds: trackIds);
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries-batch";
        PlaylistEntriesAddedDto expectedResult = _playlistEntriesAddedDtoFixture.Create(playlistId: playlistId, addedCount: 2);
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
        Assert.Equal(expectedResult.AddedCount, data.GetProperty("addedCount").GetInt32());
        Assert.Equal(expectedResult.UpdatedOnUtc, data.GetProperty("updatedOnUtc").GetDateTime());
        List<(string Endpoint, object? Data)> addRequests = GetAddRequests();
        (string Endpoint, object? Data) addRequestCall = Assert.Single(addRequests);
        Assert.Equal(expectedEndpoint, addRequestCall.Endpoint);
        AddPlaylistEntriesRequest forwardedPayload = Assert.IsType<AddPlaylistEntriesRequest>(addRequestCall.Data);
        Assert.Equal(trackIds, forwardedPayload.TrackIds);
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenApiRejectsTheEntries_ShouldReturnAFailureEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries-batch";
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostException(expectedEndpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Validation", detail: "PlaylistEntryCannotBeNull"),
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
    public async Task AddPlaylistEntries_WhenPlaylistDoesNotExist_ShouldReturnNotFoundEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries-batch";
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
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
    public async Task AddPlaylistEntries_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest(playlistId.ToString(), request, antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetAddRequests());
    }

    [Fact]
    public async Task AddPlaylistEntries_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/playlists/{ZERO_GUID}/entries-batch";
        AddPlaylistEntriesRequest request = _addPlaylistEntriesRequestFixture.Create();
        _apiFactory.ApiClientStub.RegisterPostResponse(expectedEndpoint, _playlistEntriesAddedDtoFixture.Create(playlistId: Guid.Empty));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage addRequest = CreateAddRequest("not-a-guid", request, webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(addRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetAddRequests().Select(addRequestCall => addRequestCall.Endpoint));
        Assert.DoesNotContain("not-a-guid", string.Join(",", GetAddRequests().Select(addRequestCall => addRequestCall.Endpoint)), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the add entries requests that were sent upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream add entries requests that were sent.</returns>
    private List<(string Endpoint, object? Data)> GetAddRequests()
    {
        return [.. _apiFactory.ApiClientStub.PostRequests
            .Where(postRequest => postRequest.Endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal)
                && postRequest.Endpoint.EndsWith("/entries-batch", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the add entries request for the specified playlist route value.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="request">The request payload to send.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured add entries request.</returns>
    private static HttpRequestMessage CreateAddRequest(string playlistId, AddPlaylistEntriesRequest request, string? antiforgeryToken)
    {
        HttpRequestMessage addRequest = new(HttpMethod.Post, $"/en-us/playlists/item/{Uri.EscapeDataString(playlistId)}/api-add-entries")
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
