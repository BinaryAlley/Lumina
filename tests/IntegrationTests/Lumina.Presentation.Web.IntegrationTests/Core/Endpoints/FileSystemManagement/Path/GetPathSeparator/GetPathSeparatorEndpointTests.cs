#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Path.GetPathSeparator;

/// <summary>
/// Contains integration tests for the <c>/path/api-get-path-separator</c> route served by the <see cref="GetPathSeparatorEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathSeparatorEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PathSeparatorDtoFixture _pathSeparatorDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathSeparatorEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPathSeparatorEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPathSeparator_WhenCalledByAuthenticatedUser_ShouldReturnPathSeparatorFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string expectedSeparator = "\\";
        _apiFactory.ApiClientStub.RegisterGetResponse("path/get-path-separator", _pathSeparatorDtoFixture.Create(separator: expectedSeparator));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/path/api-get-path-separator");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedSeparator, json.RootElement.GetProperty("data").GetProperty("pathSeparator").GetString());
        Assert.Contains("path/get-path-separator", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
