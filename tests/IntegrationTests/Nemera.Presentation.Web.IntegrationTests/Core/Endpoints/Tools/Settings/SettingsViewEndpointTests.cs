#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Tools.Settings;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Tools.Settings;

/// <summary>
/// Contains integration tests for the <see cref="SettingsViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SettingsViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SettingsViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SettingsView_WhenCalledByAuthenticatedUser_ShouldRenderSettingsPage()
    {
        // Arrange
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/tools/settings");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("user-settings-form", content);
    }

    [Fact]
    public async Task SettingsView_WhenCalledByAuthenticatedUser_ShouldRenderTheAudioPlayerConsumptionRouteTemplates()
    {
        // Arrange
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/tools/settings");

        // Assert
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("startConsumptionRouteTemplate", content);
        Assert.Contains("reportConsumptionProgressRouteTemplate", content);
        Assert.Contains("completeConsumptionRouteTemplate", content);
        Assert.Contains("stopConsumptionRouteTemplate", content);
        Assert.Contains("/media-consumption/api-start-consumption/{libraryId}/{mediaKind}/{mediaItemId}", content);
        Assert.Contains("/media-consumption/api-report-consumption-progress/{recordId}", content);
        Assert.Contains("/media-consumption/api-complete-consumption/{recordId}", content);
        Assert.Contains("/media-consumption/api-stop-consumption/{recordId}", content);
    }
}
