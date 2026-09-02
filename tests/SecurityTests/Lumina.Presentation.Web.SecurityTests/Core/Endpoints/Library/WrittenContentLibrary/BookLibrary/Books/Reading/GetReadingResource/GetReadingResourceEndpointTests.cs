#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Reading.GetReadingResource;

/// <summary>
/// Contains security tests for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-get-reading-resource</c> route served by the <see cref="GetReadingResourceEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingResourceEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly BlobDataDtoFixture _blobDataDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetReadingResourceEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetReadingResourceEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetReadingResource_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-reading-resource?resourceKey=cover");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("../../etc/passwd")] // directory traversal
    public async Task GetReadingResource_WhenCalledWithMaliciousResourceKey_ShouldRemainSecure(string maliciousResourceKey)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterBlobResponse(_blobDataDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-reading-resource?resourceKey={Uri.EscapeDataString(maliciousResourceKey)}");
        byte[] body = await response.Content.ReadAsByteArrayAsync();

        // Assert
        Assert.DoesNotContain("etc/passwd", System.Text.Encoding.UTF8.GetString(body), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", System.Text.Encoding.UTF8.GetString(body), StringComparison.Ordinal);
        Assert.DoesNotContain(AppContext.BaseDirectory, System.Text.Encoding.UTF8.GetString(body), StringComparison.OrdinalIgnoreCase);
    }
}
