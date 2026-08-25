#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SetMetadataProviderEnabled;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.SetMetadataProviderEnabled;

/// <summary>
/// Contains security tests for the <c>/{culture}/libraries/manage/api-set-metadata-provider-enabled</c> route served by the <see cref="SetMetadataProviderEnabledEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetMetadataProviderEnabledEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly SetLibraryMetadataProviderEnabledRequestFixture _setLibraryMetadataProviderEnabledRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SetMetadataProviderEnabledEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SetMetadataProviderEnabledEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SetMetadataProviderEnabled_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        HttpRequestMessage updateRequest = CreateUpdateRequest(libraryId, pluginId);
        updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/metadata-providers/{pluginId}/enabled");
    }

    [Fact]
    public async Task SetMetadataProviderEnabled_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        HttpRequestMessage updateRequest = CreateUpdateRequest(libraryId, pluginId);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/metadata-providers/{pluginId}/enabled");
    }

    /// <summary>
    /// Builds the update request that enables the metadata provider of the library identified by <paramref name="libraryId"/> and <paramref name="pluginId"/>.
    /// </summary>
    /// <param name="libraryId">The Id of the media library.</param>
    /// <param name="pluginId">The Id of the plugin providing the metadata.</param>
    /// <returns>The configured update request.</returns>
    private HttpRequestMessage CreateUpdateRequest(Guid libraryId, Guid pluginId)
    {
        SetLibraryMetadataProviderEnabledRequest request = _setLibraryMetadataProviderEnabledRequestFixture.Create(
            libraryId: libraryId,
            pluginId: pluginId,
            isEnabled: true
        );
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/libraries/manage/api-set-metadata-provider-enabled")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return updateRequest;
    }
}
