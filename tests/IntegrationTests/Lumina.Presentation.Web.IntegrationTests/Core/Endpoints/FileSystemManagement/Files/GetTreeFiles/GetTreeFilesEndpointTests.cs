#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Files.GetTreeFiles;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Files.GetTreeFiles;

/// <summary>
/// Contains integration tests for the <c>/files/api-get-tree-files</c> route served by the <see cref="GetTreeFilesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly FileSystemTreeNodeDtoFixture _fileSystemTreeNodeDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetTreeFilesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetTreeFiles_WhenCalledByAuthenticatedUser_ShouldReturnFileSystemTreeFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string path = @"C:\Users\test";
        FileSystemTreeNodeDto[] expectedNodes = [_fileSystemTreeNodeDtoFixture.Create(path: path, name: "test")];
        string expectedEndpoint = $"files/get-tree-files?path={Uri.EscapeDataString(path)}&includeHiddenElements=True";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedNodes);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/files/api-get-tree-files?path={Uri.EscapeDataString(path)}&includeHiddenElements=true");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedNodes.Length, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
