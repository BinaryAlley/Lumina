#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeBookReaders;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.GetLibraryTypeBookReaders;

/// <summary>
/// Contains security tests for the <see cref="GetLibraryTypeBookReadersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeBookReadersEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeBookReadersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetLibraryTypeBookReadersEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetLibraryTypeBookReaders_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/libraries/manage/api-get-library-type-book-readers/Book");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.Contains("book-readers", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("' OR '1'='1")] // Basic SQL injection.
    [InlineData("'; DROP TABLE Libraries--")] // Destructive injection.
    public async Task GetLibraryTypeBookReaders_WhenCalledWithInjectionInLibraryType_ShouldRemainSecure(string maliciousLibraryType)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/book-readers/{Uri.EscapeDataString(maliciousLibraryType)}", Array.Empty<LibraryBookReaderDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/libraries/manage/api-get-library-type-book-readers/{Uri.EscapeDataString(maliciousLibraryType)}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
    }
}
