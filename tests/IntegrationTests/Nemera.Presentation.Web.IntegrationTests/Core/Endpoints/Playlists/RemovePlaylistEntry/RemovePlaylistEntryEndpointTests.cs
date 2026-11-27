#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.RemovePlaylistEntry;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// Contains integration tests for the <see cref="RemovePlaylistEntryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemovePlaylistEntryEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RemovePlaylistEntryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheDeleteAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries/{playlistEntryId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(playlistId.ToString(), playlistEntryId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        Assert.Equal([expectedEndpoint], GetRemoveEndpointCalls());
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenTheEntryDoesNotExist_ShouldReturnNotFoundEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        Guid playlistEntryId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries/{playlistEntryId}";
        _apiFactory.ApiClientStub.RegisterDeleteException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound, expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(playlistId.ToString(), playlistEntryId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NotFound", json.RootElement.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetRemoveEndpointCalls());
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenRouteIdsAreNotGuids_ShouldForwardTheEmptyGuids()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/playlists/{ZERO_GUID}/entries/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest("not-a-guid", "also-not-a-guid", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetRemoveEndpointCalls());
        Assert.DoesNotContain("not-a-guid", string.Join(",", GetRemoveEndpointCalls()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemovePlaylistEntry_WhenRouteIdsAreUppercaseGuids_ShouldForwardThemInLowercase()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        Guid playlistEntryId = Guid.Parse("FEDCBAFE-DCBA-FEDC-BAFE-DCBAFEDCBAFE");
        string expectedEndpoint = $"users/me/playlists/{playlistId}/entries/{playlistEntryId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(
            playlistId.ToString().ToUpperInvariant(),
            playlistEntryId.ToString().ToUpperInvariant(),
            webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> removeEndpoints = GetRemoveEndpointCalls();
        Assert.Equal([expectedEndpoint], removeEndpoints);
        Assert.DoesNotContain(playlistEntryId.ToString().ToUpperInvariant(), string.Join(",", removeEndpoints), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the remove entry endpoints that were called upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream remove entry endpoints that were called.</returns>
    private List<string> GetRemoveEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.DeleteEndpointsCalled
            .Where(endpoint => endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal)
                && endpoint.Contains("/entries/", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the remove entry request for the specified route values.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="playlistEntryId">The value of the playlist entry identifier route segment.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured remove entry request.</returns>
    private static HttpRequestMessage CreateRemoveRequest(string playlistId, string playlistEntryId, string? antiforgeryToken)
    {
        HttpRequestMessage removeRequest = new(HttpMethod.Delete, $"/en-us/playlists/item/{Uri.EscapeDataString(playlistId)}/entries/{Uri.EscapeDataString(playlistEntryId)}/api-remove-entry")
        {
            // The front-end sends the JSON content type even for DELETE requests, which triggers the antiforgery validation.
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        removeRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        removeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            removeRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return removeRequest;
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
