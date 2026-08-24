#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.GetEnabledLibraries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.GetEnabledLibraries;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-get-enabled-libraries</c> route served by the <see cref="GetEnabledLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetEnabledLibrariesEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetEnabledLibrariesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetEnabledLibrariesEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetEnabledLibraries_WhenCalledByAuthenticatedUser_ShouldReturnEnabledLibrariesJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        LibraryDto[] expectedLibraries =
        [
            _libraryDtoFixture.Create(id: Guid.NewGuid(), title: "Books", libraryType: "Book"),
            _libraryDtoFixture.Create(id: Guid.NewGuid(), title: "Movies", libraryType: "Video")
        ];
        _apiFactory.ApiClientStub.RegisterGetResponse("libraries/enabled", expectedLibraries);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/libraries/manage/api-get-enabled-libraries");
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
        Assert.Equal("Books", data[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetEnabledLibraries_WhenCalled_ShouldRequestEnabledLibrariesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse("libraries/enabled", Array.Empty<LibraryDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, "/en-us/libraries/manage/api-get-enabled-libraries");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("libraries/enabled", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
