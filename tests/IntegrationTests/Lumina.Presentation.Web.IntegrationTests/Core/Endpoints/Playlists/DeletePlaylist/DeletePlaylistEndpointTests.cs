#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Playlists.DeletePlaylist;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Playlists.DeletePlaylist;

/// <summary>
/// Contains integration tests for the <see cref="DeletePlaylistEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletePlaylistEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeletePlaylistEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DeletePlaylist_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheDeleteAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(playlistId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
        Assert.Equal([expectedEndpoint], GetDeleteEndpointCalls());
    }

    [Fact]
    public async Task DeletePlaylist_WhenPlaylistDoesNotExist_ShouldReturnNotFoundEnvelope()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterDeleteException(expectedEndpoint, new ApiException(null, HttpStatusCode.NotFound, expectedEndpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(playlistId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal("NotFound", json.RootElement.GetProperty("errorMessage").GetString());
        Assert.False(json.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public async Task DeletePlaylist_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallTheApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(playlistId.ToString(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetDeleteEndpointCalls());
    }

    [Fact]
    public async Task DeletePlaylist_WhenPlaylistIdIsNotAGuid_ShouldForwardTheEmptyGuid()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"users/me/playlists/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest("not-a-guid", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal([expectedEndpoint], GetDeleteEndpointCalls());
        Assert.DoesNotContain("not-a-guid", string.Join(",", GetDeleteEndpointCalls()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeletePlaylist_WhenPlaylistIdIsAnUppercaseGuid_ShouldForwardItInLowercase()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid playlistId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        string expectedEndpoint = $"users/me/playlists/{playlistId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage deleteRequest = CreateDeleteRequest(playlistId.ToString().ToUpperInvariant(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(deleteRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> deleteEndpoints = GetDeleteEndpointCalls();
        Assert.Equal([expectedEndpoint], deleteEndpoints);
        Assert.DoesNotContain(playlistId.ToString().ToUpperInvariant(), string.Join(",", deleteEndpoints), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the delete playlist endpoints that were called upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream delete playlist endpoints that were called.</returns>
    private List<string> GetDeleteEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.DeleteEndpointsCalled
            .Where(endpoint => endpoint.StartsWith("users/me/playlists/", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the delete playlist request for the specified route value.
    /// </summary>
    /// <param name="playlistId">The value of the playlist identifier route segment.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured delete playlist request.</returns>
    private static HttpRequestMessage CreateDeleteRequest(string playlistId, string? antiforgeryToken)
    {
        HttpRequestMessage deleteRequest = new(HttpMethod.Delete, $"/en-us/playlists/item/{Uri.EscapeDataString(playlistId)}/api-delete-playlist")
        {
            // The front-end sends the JSON content type even for DELETE requests, which triggers the antiforgery validation.
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        deleteRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        deleteRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (antiforgeryToken is not null)
            deleteRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return deleteRequest;
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
