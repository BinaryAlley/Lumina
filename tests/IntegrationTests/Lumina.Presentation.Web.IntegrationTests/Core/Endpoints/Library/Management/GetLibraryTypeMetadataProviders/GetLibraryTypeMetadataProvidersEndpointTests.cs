#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetLibraryTypeMetadataProviders;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.GetLibraryTypeMetadataProviders;

/// <summary>
/// Contains integration tests for the <see cref="GetLibraryTypeMetadataProvidersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryTypeMetadataProvidersEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryMetadataProviderDtoFixture _libraryMetadataProviderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryTypeMetadataProvidersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetLibraryTypeMetadataProvidersEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetLibraryTypeMetadataProviders_WhenCalledByAuthenticatedUser_ShouldReturnProvidersJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string libraryType = "Book";
        LibraryMetadataProviderDto[] expectedProviders =
        [
            _libraryMetadataProviderDtoFixture.Create(name: "EPUB Reader", isEnabled: false)
        ];
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/metadata-providers/{libraryType}", expectedProviders);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-library-type-metadata-providers/{libraryType}");
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
        Assert.Equal(1, data.GetArrayLength());
        Assert.Equal("EPUB Reader", data[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetLibraryTypeMetadataProviders_WhenCalled_ShouldRequestProvidersFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        string libraryType = "Book";
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/metadata-providers/{libraryType}", Array.Empty<LibraryMetadataProviderDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-library-type-metadata-providers/{libraryType}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains($"libraries/metadata-providers/{libraryType}", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetLibraryTypeMetadataProviders_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync("/en-us/libraries/manage/api-get-library-type-metadata-providers/Book");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
