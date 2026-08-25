#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// Contains security tests for the <c>/directories/api-get-directories</c> route served by the <see cref="GetDirectoriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetDirectoriesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetDirectories_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/directories/api-get-directories");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("directories/get-directories", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("'; DROP TABLE Directories--")] // destructive SQL injection
    [InlineData("' OR '1'='1")] // basic SQL injection
    public async Task GetDirectories_WhenCalledWithInjectionInPath_ShouldRemainSecure(string maliciousPath)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"directories/get-directories?path={Uri.EscapeDataString(maliciousPath)}&includeHiddenElements=False";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, Array.Empty<DirectoryDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/directories/api-get-directories?path={Uri.EscapeDataString(maliciousPath)}&includeHiddenElements=false");
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
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
