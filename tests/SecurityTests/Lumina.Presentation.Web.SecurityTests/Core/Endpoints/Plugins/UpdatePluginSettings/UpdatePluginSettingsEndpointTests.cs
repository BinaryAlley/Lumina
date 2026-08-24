#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Requests.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.UpdatePluginSettings;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Plugins;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Plugins.UpdatePluginSettings;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/manage-plugins/api-update-plugin-settings</c> route served by the <see cref="UpdatePluginSettingsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePluginSettingsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();
    private readonly UpdatePluginSettingsRequestFixture _updatePluginSettingsRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePluginSettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public UpdatePluginSettingsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task UpdatePluginSettings_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/admin/manage-plugins/api-update-plugin-settings")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"plugins/{request.PluginId}/settings");
    }

    [Fact]
    public async Task UpdatePluginSettings_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create();
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/admin/manage-plugins/api-update-plugin-settings")
        {
            Content = JsonContent.Create(request)
        };
        updateRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        updateRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"plugins/{request.PluginId}/settings");
    }

    [Fact]
    public async Task UpdatePluginSettings_WhenApiRejectsUpdate_ShouldReturnCleanError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        UpdatePluginSettingsRequest request = _updatePluginSettingsRequestFixture.Create();
        string endpoint = $"plugins/{request.PluginId}/settings";
        _apiFactory.ApiClientStub.RegisterPutException(endpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Failure", detail: "PluginNotFound"),
            HttpStatusCode.NotFound));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage updateRequest = new(HttpMethod.Put, "/en-us/admin/manage-plugins/api-update-plugin-settings")
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
        Assert.False(response.IsSuccessStatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }
}
