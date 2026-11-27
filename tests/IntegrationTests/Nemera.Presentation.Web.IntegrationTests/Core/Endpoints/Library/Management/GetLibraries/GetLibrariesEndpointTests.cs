#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Libraries;
using Nemera.Presentation.Web.Core.Endpoints.Library.Management.GetLibraries;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.Management.GetLibraries;

/// <summary>
/// Contains integration tests for the <see cref="GetLibrariesEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibrariesEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibrariesEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetLibrariesEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetLibraries_WhenCalledByAuthenticatedUser_ShouldReturnLibrariesJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        LibraryDto[] expectedLibraries =
        [
            _libraryDtoFixture.Create(id: Guid.NewGuid(), title: "Books", libraryType: "Book"),
            _libraryDtoFixture.Create(id: Guid.NewGuid(), title: "Movies", libraryType: "Video")
        ];
        _apiFactory.ApiClientStub.RegisterGetResponse("libraries", expectedLibraries);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/libraries/manage/api-get-libraries");
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
    public async Task GetLibraries_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient client = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await client.GetAsync("/en-us/libraries/manage/api-get-libraries");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost/en-us/auth/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task GetLibraries_WhenCalled_ShouldRequestLibrariesFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.RegisterGetResponse("libraries", Array.Empty<LibraryDto>());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/libraries/manage/api-get-libraries");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("libraries", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
