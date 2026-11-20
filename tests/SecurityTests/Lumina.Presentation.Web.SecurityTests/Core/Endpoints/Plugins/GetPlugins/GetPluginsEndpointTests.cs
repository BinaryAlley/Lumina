#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.GetPlugins;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Plugins.GetPlugins;

/// <summary>
/// Contains security tests for the <see cref="GetPluginsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPluginsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();
    private readonly PluginDtoFixture _pluginDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPluginsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPluginsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPlugins_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/manage-plugins/api-get-plugins");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "plugins");
    }

    [Fact]
    public async Task GetPlugins_WhenCalledByNonAdminUser_ShouldRedirectToAccessDenied()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: Enum.GetValues<AuthorizationPermission>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/manage-plugins/api-get-plugins");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/en-us/auth/access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint == "plugins");
    }

    [Fact]
    public async Task GetPlugins_WhenCalledByAdminUser_ShouldReturnPluginsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
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

    [Fact]
    public async Task GetPlugins_WhenApiReturnsError_ShouldReturnCleanError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: Enum.GetValues<AuthorizationPermission>());
        _apiFactory.ApiClientStub.RegisterGetException("plugins", new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Failure", detail: "PluginNotFound"),
            HttpStatusCode.NotFound));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/admin/manage-plugins/api-get-plugins");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }
}
