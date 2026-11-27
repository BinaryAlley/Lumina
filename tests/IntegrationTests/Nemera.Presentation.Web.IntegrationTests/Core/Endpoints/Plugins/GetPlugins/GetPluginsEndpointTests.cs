#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Plugins;
using Nemera.Presentation.Web.Core.Endpoints.Plugins.GetPlugins;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// Contains integration tests for the <see cref="GetPluginsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly PluginDtoFixture _pluginDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPluginsEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPlugins_WhenCalledByAuthenticatedAdmin_ShouldReturnPluginsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        PluginDto[] expectedPlugins = [_pluginDtoFixture.Create(name: "OpenLibrary")];
        _apiFactory.ApiClientStub.RegisterGetResponse("plugins", expectedPlugins);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/manage-plugins/api-get-plugins");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedPlugins.Length, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "plugins");
    }
}
