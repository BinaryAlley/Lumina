#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.FileSystem;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.FileSystem.GetType;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.FileSystem.GetType;

/// <summary>
/// Contains integration tests for the <c>/file-system/api-get-type</c> route served by the <see cref="GetTypeEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTypeEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly FileSystemTypeDtoFixture _fileSystemTypeDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTypeEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetTypeEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetType_WhenCalledByAuthenticatedUser_ShouldReturnFileSystemTypeFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse("file-system/get-type", _fileSystemTypeDtoFixture.Create(platformType: PlatformType.Windows));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/file-system/api-get-type");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(nameof(PlatformType.Windows), json.RootElement.GetProperty("data").GetProperty("platformType").GetString());
        Assert.Contains("file-system/get-type", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
