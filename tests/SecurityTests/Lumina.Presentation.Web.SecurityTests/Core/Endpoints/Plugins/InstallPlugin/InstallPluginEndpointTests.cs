#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Plugins.InstallPlugin;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Plugins.InstallPlugin;

/// <summary>
/// Contains security tests for the <c>/{culture}/admin/manage-plugins/api-install-plugin</c> route served by the <see cref="InstallPluginEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallPluginEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public InstallPluginEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
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

    [Fact]
    public async Task InstallPlugin_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage installRequest = new(HttpMethod.Post, "/en-us/admin/manage-plugins/api-install-plugin")
        {
            Content = CreateMultipartContent("plugin.zip")
        };
        installRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(installRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "plugins");
    }

    [Fact]
    public async Task InstallPlugin_WhenCalledWithoutFilePart_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage installRequest = new(HttpMethod.Post, "/en-us/admin/manage-plugins/api-install-plugin")
        {
            Content = CreateEmptyMultipartContent()
        };
        installRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        installRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(installRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "plugins");
    }

    [Fact]
    public async Task InstallPlugin_WhenApiRejectsUpload_ShouldReturnCleanError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostException("plugins", new ApiException(
            new ProblemDetailsDto { Title = "General.Failure", Detail = "PluginArchiveNotReadable" },
            HttpStatusCode.Forbidden));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage installRequest = new(HttpMethod.Post, "/en-us/admin/manage-plugins/api-install-plugin")
        {
            Content = CreateMultipartContent("plugin.zip")
        };
        installRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        installRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(installRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
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

    /// <summary>
    /// Builds a multipart form content without any file part.
    /// </summary>
    /// <returns>The multipart form content.</returns>
    private static MultipartFormDataContent CreateEmptyMultipartContent()
    {
        MultipartFormDataContent form = [];
        form.Add(new StringContent("value"), "field");
        return form;
    }
}
