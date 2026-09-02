#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Plugins;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetBookReaders;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.GetBookReaders;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-get-book-readers/{{libraryId}}</c> route served by the <see cref="GetBookReadersEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookReadersEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryBookReaderDtoFixture _libraryBookReaderDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookReadersEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetBookReadersEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetBookReaders_WhenCalledByAuthenticatedUser_ShouldReturnBookReadersJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryBookReaderDto[] expectedReaders =
        [
            _libraryBookReaderDtoFixture.Create(name: "EPUB Reader", isEnabled: true)
        ];
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/book-readers", expectedReaders);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-book-readers/{libraryId}");
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
    public async Task GetBookReaders_WhenCalled_ShouldRequestBookReadersFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/book-readers", Array.Empty<LibraryBookReaderDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/libraries/manage/api-get-book-readers/{libraryId}");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains($"libraries/{libraryId}/book-readers", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetBookReaders_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/libraries/manage/api-get-book-readers/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
