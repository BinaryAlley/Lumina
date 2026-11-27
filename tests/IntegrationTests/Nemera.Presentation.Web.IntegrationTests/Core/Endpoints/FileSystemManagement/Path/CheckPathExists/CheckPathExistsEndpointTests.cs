#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;
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

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Path.CheckPathExists;

/// <summary>
/// Contains integration tests for the <see cref="CheckPathExistsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly PathExistsDtoFixture _pathExistsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public CheckPathExistsEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task CheckPathExists_WhenCalledByAuthenticatedUser_ShouldReturnExistenceFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string path = @"C:\Users\test";
        string expectedEndpoint = $"path/check-path-exists?path={Uri.EscapeDataString(path)}&shouldIncludeHiddenElements=True";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _pathExistsDtoFixture.Create(exists: true));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/path/api-check-path-exists?path={Uri.EscapeDataString(path)}&shouldIncludeHiddenElements=true");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("exists").GetBoolean());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
