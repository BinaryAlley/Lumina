#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetArtworkProviders;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.GetArtworkProviders;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-get-artwork-providers/{{libraryId}}</c> route served by the <see cref="GetArtworkProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtworkProvidersEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryArtworkProviderDtoFixture _libraryArtworkProviderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtworkProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetArtworkProvidersEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetArtworkProviders_WhenCalledByAuthenticatedUser_ShouldReturnArtworkProvidersJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryArtworkProviderDto[] expectedProviders =
        [
            _libraryArtworkProviderDtoFixture.Create(name: "Artwork Provider One", isEnabled: true, rank: 1),
            _libraryArtworkProviderDtoFixture.Create(name: "Artwork Provider Two", isEnabled: false, rank: 2)
        ];
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artwork-providers", expectedProviders);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-artwork-providers/{libraryId}");
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
        Assert.Equal("Artwork Provider One", data[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetArtworkProviders_WhenCalled_ShouldRequestArtworkProvidersFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/artwork-providers", Array.Empty<LibraryArtworkProviderDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-artwork-providers/{libraryId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains($"libraries/{libraryId}/artwork-providers", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
