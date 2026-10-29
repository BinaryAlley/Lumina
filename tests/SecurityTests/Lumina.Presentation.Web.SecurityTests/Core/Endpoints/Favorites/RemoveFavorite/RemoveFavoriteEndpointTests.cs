#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.RemoveFavorite;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// Contains security tests for the <see cref="RemoveFavoriteEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RemoveFavoriteEndpointTests : IClassFixture<LuminaWebFactory>, IDisposable
{
    private const string ZERO_GUID = "00000000-0000-0000-0000-000000000000";

    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RemoveFavoriteEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RemoveFavorite_WhenCalledWithoutAuthentication_ShouldRedirectToLoginAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage removeRequest = CreateRemoveRequest(nameof(FavoriteMediaKind.MusicTrack), Guid.NewGuid().ToString(), antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(removeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.Empty(GetRemoveEndpointCalls());
    }

    [Fact]
    public async Task RemoveFavorite_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(nameof(FavoriteMediaKind.MusicTrack), Guid.NewGuid().ToString(), antiforgeryToken: null);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetRemoveEndpointCalls());
    }

    [Fact]
    public async Task RemoveFavorite_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(nameof(FavoriteMediaKind.MusicTrack), Guid.NewGuid().ToString(), "invalid-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(GetRemoveEndpointCalls());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    [InlineData("MusicTrack/../../admin")] // path traversal through a valid media kind prefix
    public async Task RemoveFavorite_WithInjectionPayloadInMediaKind_ShouldSanitizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string sanitizedEndpoint = $"users/me/favorites/Invalid/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(sanitizedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(Uri.EscapeDataString(payload), mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route values are normalized before they are substituted into the upstream URL, so a crafted value can never escape its URL segment.
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> removeEndpoints = GetRemoveEndpointCalls();
        Assert.Equal([sanitizedEndpoint], removeEndpoints);
        Assert.DoesNotContain(payload, string.Join(",", removeEndpoints), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", removeEndpoints), StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Favorites--")] // destructive injection
    [InlineData("../../admin")] // path traversal
    [InlineData("<script>alert(1)</script>")] // script injection
    public async Task RemoveFavorite_WithInjectionPayloadInMediaItemId_ShouldSanitizeItAndNotForwardTheRawValue(string payload)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string sanitizedEndpoint = $"users/me/favorites/MusicTrack/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(sanitizedEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(nameof(FavoriteMediaKind.MusicTrack), Uri.EscapeDataString(payload), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        List<string> removeEndpoints = GetRemoveEndpointCalls();
        Assert.Equal([sanitizedEndpoint], removeEndpoints);
        Assert.DoesNotContain(payload, string.Join(",", removeEndpoints), StringComparison.Ordinal);
        Assert.DoesNotContain(Uri.EscapeDataString(payload), string.Join(",", removeEndpoints), StringComparison.Ordinal);
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemoveFavorite_WhenCalledByAuthenticatedUserWithValidToken_ShouldNotLeakSensitiveData()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.NewGuid();
        string expectedUpstreamEndpoint = $"users/me/favorites/MusicTrack/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedUpstreamEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest(nameof(FavoriteMediaKind.MusicTrack), mediaItemId.ToString(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expectedUpstreamEndpoint], GetRemoveEndpointCalls());
        Assert.DoesNotContain("test_jwt_token", content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at Lumina", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the remove favorite endpoints that were called upstream, excluding the requests made to authenticate the test client.
    /// </summary>
    /// <returns>The list of upstream remove favorite endpoints that were called.</returns>
    private List<string> GetRemoveEndpointCalls()
    {
        return [.. _apiFactory.ApiClientStub.DeleteEndpointsCalled
            .Where(endpoint => endpoint.StartsWith("users/me/favorites", StringComparison.Ordinal))];
    }

    /// <summary>
    /// Builds the remove favorite request for the specified route values.
    /// </summary>
    /// <param name="mediaKind">The value of the media kind route segment.</param>
    /// <param name="mediaItemId">The value of the media item identifier route segment.</param>
    /// <param name="antiforgeryToken">The antiforgery token to include in the request, or <see langword="null"/> to omit it.</param>
    /// <returns>The configured remove favorite request.</returns>
    private static HttpRequestMessage CreateRemoveRequest(string mediaKind, string mediaItemId, string? antiforgeryToken)
    {
        HttpRequestMessage removeRequest = new(HttpMethod.Delete, $"/en-us/favorites/{mediaKind}/{mediaItemId}/api-remove-favorite")
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
