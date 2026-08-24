#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Themes.ThemeAssets;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Themes.ThemeAssets;

/// <summary>
/// Contains security tests for the <c>/theme-assets/{themeId}/{path}</c> route served by the <see cref="ThemeAssetsEndpoint"/> class.
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
    public async Task ThemeAssets_WhenCalledWithoutAuthentication_ShouldServeAsset()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        byte[] expectedBytes = [0x89, 0x50, 0x4E, 0x47];
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create(data: expectedBytes, contentType: "image/png"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/theme-assets/test-theme/assets/logo.png");
        byte[] content = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(expectedBytes, content);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Themes--")] // destructive injection
    public async Task ThemeAssets_WithInjectionInThemeId_ShouldNotLeakOrError(string maliciousThemeId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create(data: [0x01], contentType: "application/octet-stream"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/theme-assets/{Uri.EscapeDataString(maliciousThemeId)}/assets/logo.png");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("..\\..\\appsettings.json")] // path traversal
    public async Task ThemeAssets_WithInjectionInPath_ShouldNotLeakOrError(string maliciousPath)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create(data: [0x01], contentType: "application/octet-stream"));
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/theme-assets/test-theme/assets/{Uri.EscapeDataString(maliciousPath)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }
}
