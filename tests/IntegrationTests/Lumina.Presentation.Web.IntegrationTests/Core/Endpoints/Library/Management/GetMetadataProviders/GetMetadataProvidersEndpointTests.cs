#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetMetadataProviders;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Plugins;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.GetMetadataProviders;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-get-metadata-providers/{{libraryId}}</c> route served by the <see cref="GetMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMetadataProvidersEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryMetadataProviderDtoFixture _libraryMetadataProviderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMetadataProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetMetadataProvidersEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetMetadataProviders_WhenCalledByAuthenticatedUser_ShouldReturnMetadataProvidersJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderDto[] expectedProviders =
        [
            _libraryMetadataProviderDtoFixture.Create(name: "Metadata Provider One", isEnabled: true, rank: 1),
            _libraryMetadataProviderDtoFixture.Create(name: "Metadata Provider Two", isEnabled: false, rank: 2)
        ];
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/metadata-providers", expectedProviders);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-metadata-providers/{libraryId}");
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
        Assert.Equal("Metadata Provider One", data[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetMetadataProviders_WhenCalled_ShouldRequestMetadataProvidersFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/metadata-providers", Array.Empty<LibraryMetadataProviderDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-metadata-providers/{libraryId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains($"libraries/{libraryId}/metadata-providers", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
