#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.FileSystemManagement.Path.ValidatePath;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.FileSystemManagement.Path.ValidatePath;

/// <summary>
/// Contains integration tests for the <c>/path/api-validate</c> route served by the <see cref="ValidatePathEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ValidatePathEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PathValidDtoFixture _pathValidDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public ValidatePathEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task ValidatePath_WhenCalledByAuthenticatedUser_ShouldReturnValidityFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string path = @"C:\Users\test";
        string expectedEndpoint = $"path/validate?path={Uri.EscapeDataString(path)}";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, _pathValidDtoFixture.Create(isValid: true));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/path/api-validate?path={Uri.EscapeDataString(path)}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("isValid").GetBoolean());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
