#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Core.Endpoints.Favorites.RemoveFavorite;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// Contains integration tests for the <see cref="RemoveFavoriteEndpoint"/> class.
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
    public async Task RemoveFavorite_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardTheNormalizedDeleteAndReturnSuccess()
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
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal([expectedUpstreamEndpoint], GetRemoveEndpointCalls());
    }

    [Fact]
    public async Task RemoveFavorite_WhenMediaKindAndIdAreCasedDifferently_ShouldForwardTheCanonicalValuesOnly()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid mediaItemId = Guid.Parse("ABCDEFAB-CDEF-ABCD-EFAB-CDEFABCDEFAB");
        string expectedUpstreamEndpoint = $"users/me/favorites/MusicTrack/{mediaItemId}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedUpstreamEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest("musictrack", mediaItemId.ToString().ToUpperInvariant(), webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> removeEndpoints = GetRemoveEndpointCalls();
        Assert.Equal([expectedUpstreamEndpoint], removeEndpoints);
        Assert.DoesNotContain(mediaItemId.ToString().ToUpperInvariant(), string.Join(",", removeEndpoints), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemoveFavorite_WhenRouteValuesAreCrafted_ShouldForwardTheSanitizedPlaceholders()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedUpstreamEndpoint = $"users/me/favorites/Invalid/{ZERO_GUID}";
        _apiFactory.ApiClientStub.RegisterDeleteSuccess(expectedUpstreamEndpoint);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage removeRequest = CreateRemoveRequest("notamediakind", "not-a-guid", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(removeRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        List<string> removeEndpoints = GetRemoveEndpointCalls();
        Assert.Equal([expectedUpstreamEndpoint], removeEndpoints);
        Assert.DoesNotContain("notamediakind", string.Join(",", removeEndpoints), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not-a-guid", string.Join(",", removeEndpoints), StringComparison.OrdinalIgnoreCase);
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
    /// <param name="antiforgeryToken">The antiforgery token to include in the request.</param>
    /// <returns>The configured remove favorite request.</returns>
    private static HttpRequestMessage CreateRemoveRequest(string mediaKind, string mediaItemId, string antiforgeryToken)
    {
        HttpRequestMessage removeRequest = new(HttpMethod.Delete, $"/en-us/favorites/{Uri.EscapeDataString(mediaKind)}/{Uri.EscapeDataString(mediaItemId)}/api-remove-favorite")
        {
            // The front-end sends the JSON content type even for DELETE requests, which triggers the antiforgery validation.
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        removeRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        removeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
