#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetRunningLibraryScans;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.GetRunningLibraryScans;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-get-running-library-scans</c> route served by the <see cref="GetRunningLibraryScansEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetRunningLibraryScansEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryScanProgressDtoFixture _libraryScanProgressDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRunningLibraryScansEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetRunningLibraryScansEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetRunningLibraryScans_WhenCalledByAuthenticatedUser_ShouldReturnRunningLibraryScansJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        LibraryScanProgressDto[] expectedScans =
        [
            _libraryScanProgressDtoFixture.Create(includeCurrentJobProgress: true),
            _libraryScanProgressDtoFixture.Create()
        ];
        _apiFactory.ApiClientStub.RegisterGetResponse("libraries/scans/running", expectedScans);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/libraries/manage/api-get-running-library-scans");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(2, data.GetArrayLength());
        Assert.Equal(JsonValueKind.Object, data[0].GetProperty("currentJobProgress").ValueKind);
    }

    [Fact]
    public async Task GetRunningLibraryScans_WhenCalled_ShouldRequestRunningLibraryScansFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse("libraries/scans/running", Array.Empty<LibraryScanProgressDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/libraries/manage/api-get-running-library-scans");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("libraries/scans/running", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
