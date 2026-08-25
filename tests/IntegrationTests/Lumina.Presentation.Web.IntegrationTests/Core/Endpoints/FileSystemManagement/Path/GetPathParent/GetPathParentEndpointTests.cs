#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.GetPathParent;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Path.GetPathParent;

/// <summary>
/// Contains integration tests for the <c>/path/api-get-path-parent</c> route served by the <see cref="GetPathParentEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PathSegmentDtoFixture _pathSegmentDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetPathParentEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetPathParent_WhenCalledByAuthenticatedUser_ShouldReturnPathSegmentsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string path = @"C:\Users\test";
        PathSegmentDto[] expectedSegments = [_pathSegmentDtoFixture.Create(path: @"C:\Users")];
        string expectedEndpoint = $"path/get-path-parent?path={Uri.EscapeDataString(path)}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedSegments);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/path/api-get-path-parent?path={Uri.EscapeDataString(path)}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedSegments.Length, json.RootElement.GetProperty("data").GetProperty("pathSegments").GetArrayLength());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
