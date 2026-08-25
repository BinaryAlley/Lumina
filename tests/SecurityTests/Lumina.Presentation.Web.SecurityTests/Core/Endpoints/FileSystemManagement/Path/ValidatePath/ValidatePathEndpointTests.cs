#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.ValidatePath;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// Contains security tests for the <c>/path/api-validate</c> route served by the <see cref="ValidatePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PathValidDtoFixture _pathValidDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ValidatePathEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ValidatePath_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/path/api-validate");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("path/validate", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("'; DROP TABLE Paths--")] // destructive SQL injection
    [InlineData("' OR '1'='1")] // basic SQL injection
    public async Task ValidatePath_WhenCalledWithInjectionInPath_ShouldRemainSecure(string maliciousPath)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedEndpoint = $"path/validate?path={Uri.EscapeDataString(maliciousPath)}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _pathValidDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/path/api-validate?path={Uri.EscapeDataString(maliciousPath)}");
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
