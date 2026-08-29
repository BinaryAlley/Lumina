#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.FileSystemManagement.Thumbnails.GetThumbnail;

/// <summary>
/// Contains security tests for the <c>/thumbnails/api-get-thumbnail</c> route served by the <see cref="GetThumbnailEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThumbnailEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetThumbnailEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetThumbnail_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/thumbnails/api-get-thumbnail?path=%2Fmedia%2Fphoto.png&quality=70");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("'; DROP TABLE Thumbnails--")] // destructive SQL injection
    [InlineData("..\\..\\appsettings.json")] // path traversal
    [InlineData("' OR '1'='1")] // basic SQL injection
    public async Task GetThumbnail_WhenCalledWithInjectionInPath_ShouldRemainSecure(string maliciousPath)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create(data: [0x89, 0x50, 0x4E, 0x47], contentType: "image/png"));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/thumbnails/api-get-thumbnail?path={Uri.EscapeDataString(maliciousPath)}&quality=70");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }
}
