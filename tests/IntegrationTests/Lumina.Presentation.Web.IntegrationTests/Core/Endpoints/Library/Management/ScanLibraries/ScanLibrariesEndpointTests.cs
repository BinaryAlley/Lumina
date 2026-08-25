#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibraries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.ScanLibraries;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-scan-libraries</c> route served by the <see cref="ScanLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibrariesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ScanLibraryDtoFixture _scanLibraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibrariesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ScanLibrariesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ScanLibraries_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldInitiateScanAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        ScanLibraryDto[] expectedScans =
        [
            _scanLibraryDtoFixture.Create(libraryId: libraryId)
        ];
        _apiFactory.ApiClientStub.RegisterPostResponse("libraries/scans", expectedScans);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage scanRequest = CreateScanRequest("/en-us/libraries/manage/api-scan-libraries");
        scanRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(scanRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(1, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries/scans");
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
