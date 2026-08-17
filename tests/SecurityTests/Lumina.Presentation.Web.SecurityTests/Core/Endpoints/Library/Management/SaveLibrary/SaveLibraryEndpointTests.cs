#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SaveLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.SaveLibrary;

/// <summary>
/// Contains security tests for the <c>/{culture}/libraries/manage/api-item</c> route served by the <see cref="SaveLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveLibraryEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveLibraryEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SaveLibraryEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task SaveLibrary_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(new LibraryDto { Title = "New Books Library", LibraryType = "Book" });

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries");
    }

    [Fact]
    public async Task SaveLibrary_WhenCalledWithInvalidAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(new LibraryDto { Title = "New Books Library", LibraryType = "Book" });
        saveRequest.Headers.Remove("RequestVerificationToken");
        saveRequest.Headers.Add("RequestVerificationToken", "invalid-antiforgery-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries");
    }

    [Fact]
    public async Task SaveLibrary_WhenCalledWithInjectionInLibraryTitle_ShouldRemainSecure()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        LibraryDto request = new() { Title = "'; DROP TABLE Libraries--", LibraryType = "Book" };
        _apiFactory.ApiClientStub.RegisterPostResponse("libraries", new LibraryDto { Id = Guid.NewGuid(), Title = request.Title, LibraryType = request.LibraryType });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("SQL", content);
        Assert.DoesNotContain("Exception", content);
    }

    private static HttpRequestMessage CreateSaveRequest(LibraryDto request)
    {
        HttpRequestMessage saveRequest = new(HttpMethod.Post, "/en-us/libraries/manage/api-item")
        {
            Content = JsonContent.Create(request)
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return saveRequest;
    }
}
