#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.ReorderMetadataProviders;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.ReorderMetadataProviders;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-reorder-metadata-providers</c> route served by the <see cref="ReorderMetadataProvidersEndpoint"/> class.
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
    public async Task ReorderMetadataProviders_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldReorderProvidersAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid firstPluginId = Guid.NewGuid();
        Guid secondPluginId = Guid.NewGuid();
        ReorderLibraryMetadataProvidersRequest request = _reorderLibraryMetadataProvidersRequestFixture.Create(
            libraryId: libraryId,
            pluginIds: [firstPluginId, secondPluginId]
        );
        string expectedEndpoint = $"libraries/{libraryId}/metadata-providers/reorder";
        _apiFactory.ApiClientStub.RegisterPutResponseFactory(expectedEndpoint, _ => new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/libraries/manage/api-reorder-metadata-providers")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        updateRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == expectedEndpoint);
    }
}
