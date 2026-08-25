#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.GetPluginSettings;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Plugins.GetPluginSettings;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/manage-plugins/api-get-plugin-settings/{pluginId}</c> route served by the <see cref="GetPluginSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginSettingsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PluginSettingsDtoFixture _pluginSettingsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPluginSettingsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPluginSettings_WhenCalledByAuthenticatedAdmin_ShouldReturnPluginSettingsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid pluginId = Guid.NewGuid();
        PluginSettingsDto expectedSettings = _pluginSettingsDtoFixture.Create(pluginId: pluginId);
        string expectedEndpoint = $"plugins/{pluginId}/settings";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedSettings);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/admin/manage-plugins/api-get-plugin-settings/{pluginId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(pluginId, json.RootElement.GetProperty("data").GetProperty("pluginId").GetGuid());
        Assert.Contains(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == expectedEndpoint);
    }
}
