#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.ScanLibrary;

/// <summary>
/// Contains security tests for the <c>/{culture}/libraries/manage/api-scan-library/{{id}}</c> route served by the <see cref="ScanLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ScanLibraryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ScanLibrary_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        Guid libraryId = Guid.NewGuid();
        HttpRequestMessage scanRequest = CreateScanRequest($"/en-us/libraries/manage/api-scan-library/{libraryId}");
        scanRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(scanRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == $"libraries/{libraryId}/scans");
    }

    [Fact]
    public async Task ScanLibrary_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        Guid libraryId = Guid.NewGuid();
        HttpRequestMessage scanRequest = CreateScanRequest($"/en-us/libraries/manage/api-scan-library/{libraryId}");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(scanRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == $"libraries/{libraryId}/scans");
    }

    [Theory]
    [InlineData("' OR '1'='1")] // basic SQL injection
    [InlineData("'; DROP TABLE Libraries--")] // destructive injection
    public async Task ScanLibrary_WithInjectionInId_ShouldNotLeakOrError(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage scanRequest = CreateScanRequest($"/en-us/libraries/manage/api-scan-library/{Uri.EscapeDataString(maliciousId)}");
        scanRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(scanRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // note: the {id} route parameter is Guid-typed, so the malicious value fails model binding and never reaches
        // the handler; the Web application reports the binding failure as a 200 response carrying success=false
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stack trace", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("salt", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppContext.BaseDirectory, content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint.StartsWith("libraries/", StringComparison.Ordinal));
    }

    /// <summary>
    /// Builds the scan request that posts an empty JSON body to the given <paramref name="url"/>.
    /// </summary>
    /// <param name="url">The URL of the scan endpoint.</param>
    /// <returns>The configured scan request.</returns>
    private static HttpRequestMessage CreateScanRequest(string url)
    {
        HttpRequestMessage scanRequest = new(HttpMethod.Post, url)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // the antiforgery middleware matches the content type exactly, so the charset suffix must be omitted
        scanRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        scanRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return scanRequest;
    }
}
