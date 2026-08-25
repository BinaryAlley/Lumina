#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;
using Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Directories.GetDirectories;

/// <summary>
/// Contains integration tests for the <c>/directories/api-get-directories</c> route served by the <see cref="GetDirectoriesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly DirectoryDtoFixture _directoryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetDirectoriesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetDirectories_WhenCalledByAuthenticatedUser_ShouldReturnDirectoriesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string path = @"C:\Users\test";
        DirectoryDto[] expectedDirectories = [_directoryDtoFixture.Create(path: path, name: "test")];
        string expectedEndpoint = $"directories/get-directories?path={Uri.EscapeDataString(path)}&includeHiddenElements=True";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedDirectories);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/directories/api-get-directories?path={Uri.EscapeDataString(path)}&includeHiddenElements=true");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedDirectories.Length, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
