#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SaveLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.SaveLibrary;

/// <summary>
/// Contains integration tests for the <c>/{culture}/libraries/manage/api-item</c> route served by the <see cref="SaveLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveLibraryEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SaveLibraryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SaveLibrary_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldSaveLibraryAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        LibraryDto request = _libraryDtoFixture.Create(title: "New Books Library", libraryType: "Book");
        request.Id = null;
        LibraryDto expectedResponse = _libraryDtoFixture.Create(id: Guid.NewGuid(), title: request.Title, libraryType: request.LibraryType);
        _apiFactory.ApiClientStub.RegisterPostResponse("libraries", expectedResponse);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = new(HttpMethod.Post, "/en-us/libraries/manage/api-item")
        {
            Content = JsonContent.Create(request)
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries");
    }

    [Fact]
    public async Task SaveLibrary_WhenCalledWithoutAntiforgeryToken_ShouldReturnBadRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = new(HttpMethod.Post, "/en-us/libraries/manage/api-item")
        {
            Content = JsonContent.Create(_libraryDtoFixture.Create(title: "New Books Library", libraryType: "Book"))
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries");
    }
}
