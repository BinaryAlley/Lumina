#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Drives.GetDrives;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Drives.GetDrives;

/// <summary>
/// Contains integration tests for the <c>/drives/api-get-drives</c> route served by the <see cref="GetDrivesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDrivesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly FileSystemTreeNodeDtoFixture _fileSystemTreeNodeDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDrivesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetDrivesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetDrives_WhenCalledByAuthenticatedUser_ShouldReturnDrivesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        FileSystemTreeNodeDto[] expectedDrives = [_fileSystemTreeNodeDtoFixture.Create(path: "C:\\", name: "C:")];
        _apiFactory.ApiClientStub.RegisterGetResponse("drives/get-drives", expectedDrives);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/drives/api-get-drives");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedDrives.Length, json.RootElement.GetProperty("data").GetProperty("drives").GetArrayLength());
        Assert.Contains("drives/get-drives", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
