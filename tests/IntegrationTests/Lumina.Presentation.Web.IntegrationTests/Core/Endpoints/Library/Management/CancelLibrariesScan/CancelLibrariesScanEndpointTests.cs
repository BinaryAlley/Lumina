#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.CancelLibrariesScan;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.CancelLibrariesScan;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-cancel-libraries-scan</c> route served by the <see cref="CancelLibrariesScanEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CancelLibrariesScanEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelLibrariesScanEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CancelLibrariesScanEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CancelLibrariesScan_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldCancelScansAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterPostResponseFactory("libraries/scans/cancel", _ => new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage cancelRequest = CreateCancelRequest("/en-us/libraries/manage/api-cancel-libraries-scan");
        cancelRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(cancelRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries/scans/cancel");
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
