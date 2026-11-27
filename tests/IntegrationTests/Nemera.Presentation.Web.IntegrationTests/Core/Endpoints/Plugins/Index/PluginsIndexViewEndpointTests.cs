#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Plugins;
using Nemera.Presentation.Web.Core.Endpoints.Plugins.Index;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Plugins.Index;

/// <summary>
/// Contains integration tests for the <see cref="PluginsIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginsIndexViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly PluginDtoFixture _pluginDtoFixture = new();
    private readonly PluginSettingsDtoFixture _pluginSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginsIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public PluginsIndexViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task PluginsIndex_WhenCalledByAuthenticatedAdmin_ShouldRenderPluginsManagementPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        PluginDto[] expectedPlugins = [_pluginDtoFixture.Create(name: "OpenLibrary")];
        string settingsEndpoint = $"plugins/{expectedPlugins[0].Id}/settings";
        _apiFactory.ApiClientStub.RegisterGetResponse("plugins", expectedPlugins);
        _apiFactory.ApiClientStub.RegisterGetResponse(settingsEndpoint, _pluginSettingsDtoFixture.Create(pluginId: expectedPlugins[0].Id));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/manage-plugins");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "plugins");
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == settingsEndpoint);
    }
}
