#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Themes.ThemeAssets;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Themes.ThemeAssets;

/// <summary>
/// Contains integration tests for the <c>/theme-assets/{themeId}/{path}</c> route served by the <see cref="ThemeAssetsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeAssetsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeAssetsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ThemeAssetsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ThemeAssets_WhenCalledWithValidThemeAndPath_ShouldReturnAsset()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        byte[] expectedBytes = [0x89, 0x50, 0x4E, 0x47];
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create(data: expectedBytes, contentType: "image/png"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage assetRequest = new(HttpMethod.Get, "/theme-assets/test-theme/assets/logo.png");
        assetRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await client.SendAsync(assetRequest);
        byte[] content = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(expectedBytes, content);
    }

    [Fact]
    public async Task ThemeAssets_WhenCalledWithEmptyPath_ShouldReturnBadRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/theme-assets/test-theme/");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
