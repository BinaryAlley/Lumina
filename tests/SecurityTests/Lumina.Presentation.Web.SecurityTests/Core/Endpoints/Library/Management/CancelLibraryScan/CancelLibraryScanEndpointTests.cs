#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.CancelLibraryScan;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.CancelLibraryScan;

/// <summary>
/// Contains security tests for the <c>/{culture}/libraries/manage/{{libraryId}}/api-cancel-library-scan/{{scanId}}</c> route served by the <see cref="CancelLibraryScanEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CancelLibraryScanEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CancelLibraryScan_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        string antiforgeryToken = await WebTestHelpers.GetAntiforgeryTokenAsync(client);
        Guid libraryId = Guid.NewGuid();
        Guid scanId = Guid.NewGuid();
        HttpRequestMessage cancelRequest = CreateCancelRequest($"/en-us/libraries/manage/{libraryId}/api-cancel-library-scan/{scanId}");
        cancelRequest.Headers.Add("RequestVerificationToken", antiforgeryToken);

        // Act
        HttpResponseMessage response = await client.SendAsync(cancelRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == $"libraries/{libraryId}/scans/{scanId}/cancel");
    }

    [Fact]
    public async Task CancelLibraryScan_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        Guid libraryId = Guid.NewGuid();
        Guid scanId = Guid.NewGuid();
        HttpRequestMessage cancelRequest = CreateCancelRequest($"/en-us/libraries/manage/{libraryId}/api-cancel-library-scan/{scanId}");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(cancelRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == $"libraries/{libraryId}/scans/{scanId}/cancel");
    }

    /// <summary>
    /// Builds the cancel request that posts an empty JSON body to the given <paramref name="url"/>.
    /// </summary>
    /// <param name="url">The URL of the cancel endpoint.</param>
    /// <returns>The configured cancel request.</returns>
    private static HttpRequestMessage CreateCancelRequest(string url)
    {
        HttpRequestMessage cancelRequest = new(HttpMethod.Post, url)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // the antiforgery middleware matches the content type exactly, so the charset suffix must be omitted
        cancelRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        cancelRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return cancelRequest;
    }
}
