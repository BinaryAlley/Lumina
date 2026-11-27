#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.Plugins.InstallPlugin;
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

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Contains integration tests for the <see cref="InstallPluginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly PluginDtoFixture _pluginDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public InstallPluginEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldForwardUploadAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string fileName = "plugin.zip";
        _apiFactory.ApiClientStub.RegisterPostResponse("plugins", _pluginDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage installRequest = new(HttpMethod.Post, "/en-us/admin/manage-plugins/api-install-plugin")
        {
            Content = CreateMultipartContent(fileName)
        };
        installRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        installRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(installRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "plugins" && postRequest.Data as string == fileName);
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage installRequest = new(HttpMethod.Post, "/en-us/admin/manage-plugins/api-install-plugin")
        {
            Content = CreateMultipartContent("plugin.zip")
        };
        installRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(installRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "plugins");
    }

    /// <summary>
    /// Builds a multipart form content carrying a single file part.
    /// </summary>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>The multipart form content.</returns>
    private static MultipartFormDataContent CreateMultipartContent(string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new([0x4D, 0x5A, 0x90, 0x00]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "archive", fileName);
        return form;
    }
}
