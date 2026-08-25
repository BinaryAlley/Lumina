#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Admin.Themes.DownloadTheme;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Admin.Themes.DownloadTheme;

/// <summary>
/// Contains integration tests for the <c>/{culture}/admin/themes/api-download-theme/{themeId}</c> route served by the <see cref="DownloadThemeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DownloadThemeEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadThemeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DownloadThemeEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DownloadTheme_WhenCalledByAuthenticatedAdmin_ShouldReturnThemeArchive()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        byte[] expectedBytes = [0x50, 0x4B, 0x03, 0x04, 0x01, 0x02];
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create(data: expectedBytes, contentType: "application/zip"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage downloadRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-download-theme/test-theme");
        downloadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(downloadRequest);
        byte[] content = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/zip", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(expectedBytes, content);
    }

    [Fact]
    public async Task DownloadTheme_WhenCalledWithEmptyThemeId_ShouldReturnBadRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage downloadRequest = new(HttpMethod.Get, "/en-us/admin/themes/api-download-theme/%20");
        downloadRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(downloadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
