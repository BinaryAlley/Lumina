#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Themes;
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.InstallTheme;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes.InstallTheme;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/themes/api-install-theme</c> route served by the <see cref="InstallThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class InstallThemeEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public InstallThemeEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task InstallTheme_WhenCalledByAuthenticatedAdminWithAntiforgeryToken_ShouldForwardUploadAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string fileName = "theme.zip";
        _apiFactory.ApiClientStub.RegisterPostResponse("themes", _themeResponseDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage installRequest = new(HttpMethod.Post, "/en-us/admin/themes/api-install-theme")
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
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "themes" && postRequest.Data as string == fileName);
    }

    [Fact]
    public async Task InstallTheme_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage installRequest = new(HttpMethod.Post, "/en-us/admin/themes/api-install-theme")
        {
            Content = CreateMultipartContent("theme.zip")
        };
        installRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(installRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "themes");
    }

    /// <summary>
    /// Builds a multipart form content carrying a single file part.
    /// </summary>
    /// <param name="fileName">The name of the uploaded file.</param>
    /// <returns>The multipart form content.</returns>
    private static MultipartFormDataContent CreateMultipartContent(string fileName)
    {
        MultipartFormDataContent form = [];
        ByteArrayContent fileContent = new([0x50, 0x4B, 0x03, 0x04]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(fileContent, "archive", fileName);
        return form;
    }
}
