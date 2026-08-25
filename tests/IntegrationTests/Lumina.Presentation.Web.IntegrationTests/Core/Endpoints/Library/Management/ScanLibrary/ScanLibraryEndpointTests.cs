#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.ScanLibrary;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.ScanLibrary;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-scan-library/{{id}}</c> route served by the <see cref="ScanLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ScanLibraryEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly ScanLibraryDtoFixture _scanLibraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ScanLibraryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ScanLibrary_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldInitiateScanAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        ScanLibraryDto expectedScan = _scanLibraryDtoFixture.Create(libraryId: libraryId);
        _apiFactory.ApiClientStub.RegisterPostResponse($"libraries/{libraryId}/scans", expectedScan);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage scanRequest = CreateScanRequest($"/en-us/libraries/manage/api-scan-library/{libraryId}");
        scanRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(scanRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(libraryId, json.RootElement.GetProperty("data").GetProperty("libraryId").GetGuid());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == $"libraries/{libraryId}/scans");
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
