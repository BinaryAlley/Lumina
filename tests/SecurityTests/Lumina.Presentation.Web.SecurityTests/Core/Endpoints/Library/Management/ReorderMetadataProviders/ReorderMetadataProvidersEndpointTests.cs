#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.ReorderMetadataProviders;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.ReorderMetadataProviders;

/// <summary>
/// Contains security tests for the <c>/{culture}/libraries/manage/api-reorder-metadata-providers</c> route served by the <see cref="ReorderMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReorderMetadataProvidersEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ReorderLibraryMetadataProvidersRequestFixture _reorderLibraryMetadataProvidersRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReorderMetadataProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ReorderMetadataProvidersEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ReorderMetadataProviders_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        Guid libraryId = Guid.NewGuid();
        HttpRequestMessage updateRequest = CreateUpdateRequest(libraryId);
        updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/metadata-providers/reorder");
    }

    [Fact]
    public async Task ReorderMetadataProviders_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        Guid libraryId = Guid.NewGuid();
        HttpRequestMessage updateRequest = CreateUpdateRequest(libraryId);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/metadata-providers/reorder");
    }

    /// <summary>
    /// Builds the update request that reorders the metadata providers of the library identified by <paramref name="libraryId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <returns>The configured update request.</returns>
    private HttpRequestMessage CreateUpdateRequest(Guid libraryId)
    {
        ReorderLibraryMetadataProvidersRequest request = _reorderLibraryMetadataProvidersRequestFixture.Create(
            libraryId: libraryId,
            pluginIds: [Guid.NewGuid(), Guid.NewGuid()]
        );
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/libraries/manage/api-reorder-metadata-providers")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return updateRequest;
    }
}
