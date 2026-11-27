#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.FileSystemManagement;
using Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.Files.GetFiles;
using Nemera.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Files.GetFiles;

/// <summary>
/// Contains integration tests for the <see cref="GetFilesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly FileDtoFixture _fileDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetFilesEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetFiles_WhenCalledByAuthenticatedUser_ShouldReturnFilesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string path = @"C:\Users\test";
        FileDto[] expectedFiles = [_fileDtoFixture.Create(path: path, name: "book.pdf", isHidden: true)];
        string expectedEndpoint = $"files/get-files?path={Uri.EscapeDataString(path)}&shouldIncludeHiddenElements=True";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedFiles);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/files/api-get-files?path={Uri.EscapeDataString(path)}&shouldIncludeHiddenElements=true");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedFiles.Length, json.RootElement.GetProperty("data").GetArrayLength());
        Assert.True(json.RootElement.GetProperty("data")[0].GetProperty("isHidden").GetBoolean());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
