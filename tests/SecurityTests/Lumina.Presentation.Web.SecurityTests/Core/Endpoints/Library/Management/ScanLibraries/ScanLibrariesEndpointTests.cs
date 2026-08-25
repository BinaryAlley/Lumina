#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// Contains security tests for the <c>/{culture}/libraries/manage/api-scan-libraries</c> route served by the <see cref="ScanLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibrariesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ScanLibrariesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ScanLibraries_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        HttpRequestMessage scanRequest = CreateScanRequest();
        scanRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(scanRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries/scans");
    }

    [Fact]
    public async Task ScanLibraries_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage scanRequest = CreateScanRequest();

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(scanRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries/scans");
    }

    /// <summary>
    /// Builds the scan request that posts an empty JSON body to the scan libraries endpoint.
    /// </summary>
    /// <returns>The configured scan request.</returns>
    private static HttpRequestMessage CreateScanRequest()
    {
        HttpRequestMessage scanRequest = new(HttpMethod.Post, "/en-us/libraries/manage/api-scan-libraries")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // the antiforgery middleware matches the content type exactly, so the charset suffix must be omitted
        scanRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        scanRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return scanRequest;
    }
}
