#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Plugins;
using Nemera.Presentation.Web.Common.Enums.Authorization;
using Nemera.Presentation.Web.Core.Endpoints.Plugins.Index;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Nemera.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Plugins.Index;

/// <summary>
/// Contains security tests for the <see cref="PluginsIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PluginsIndexViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
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
    public async Task PluginsIndex_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/admin/manage-plugins");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "plugins");
    }

    [Fact]
    public async Task PluginsIndex_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/admin/manage-plugins");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "plugins");
    }

    [Fact]
    public async Task PluginsIndex_WhenCalledByAdminUser_ShouldReturnPluginsPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
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
