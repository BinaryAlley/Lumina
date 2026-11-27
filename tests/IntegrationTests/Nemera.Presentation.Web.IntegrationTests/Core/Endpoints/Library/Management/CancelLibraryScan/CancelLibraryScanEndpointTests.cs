#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.Requests.Library.Management;
using Nemera.Presentation.Web.Core.Endpoints.Library.Management.CancelLibraryScan;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.CancelLibraryScan;

/// <summary>
/// Contains integration tests for the <see cref="CancelLibraryScanEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibraryScanEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibraryScanEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CancelLibraryScanEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CancelLibraryScan_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldCancelScanAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid scanId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPostPayload<CancelLibraryScanRequest>($"libraries/{libraryId}/scans/{scanId}/cancel");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage cancelRequest = CreateCancelRequest($"/en-us/libraries/manage/{libraryId}/api-cancel-library-scan/{scanId}");
        cancelRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(cancelRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest =>
            postRequest.Endpoint == $"libraries/{libraryId}/scans/{scanId}/cancel" &&
            postRequest.Data is CancelLibraryScanRequest forwardedRequest &&
            forwardedRequest.LibraryId == libraryId &&
            forwardedRequest.ScanId == scanId);
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
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        cancelRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        cancelRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return cancelRequest;
    }
}
