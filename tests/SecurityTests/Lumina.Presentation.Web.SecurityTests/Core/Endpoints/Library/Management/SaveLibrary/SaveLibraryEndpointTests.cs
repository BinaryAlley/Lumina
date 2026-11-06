#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Enums.Authorization;
using Lumina.Presentation.Web.Core.Endpoints.Library.Management.SaveLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.Responses.Authorization;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.Management.SaveLibrary;

/// <summary>
/// Contains security tests for the <see cref="SaveLibraryEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveLibraryEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly GetAuthorizationResponseFixture _getAuthorizationResponseFixture = new();

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
        ResetApiStub();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(_libraryDtoFixture.Create(title: "New Books Library", libraryType: "Book"));

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
        ResetApiStub();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(_libraryDtoFixture.Create(title: "New Books Library", libraryType: "Book"));
        saveRequest.Headers.Remove("RequestVerificationToken");
        saveRequest.Headers.Add("RequestVerificationToken", "invalid-antiforgery-token");

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries");
    }

    [Fact]
    public async Task SaveLibrary_WhenTheUserLacksTheManagePermission_ShouldRedirectToAccessDeniedWithoutSaving()
    {
        // Arrange
        ResetApiStub();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "User", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(_libraryDtoFixture.Create(title: "New Books Library", libraryType: "Book"));
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("access-denied", response.Headers.Location!.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries");
    }

    [Fact]
    public async Task SaveLibrary_WhenAnAdministratorLacksTheManagePermission_ShouldForwardTheSave()
    {
        // Arrange
        // The API lets administrators save any library, so the Web must not refuse them for lacking the permission.
        ResetApiStub();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(userId: Guid.NewGuid(), role: "Admin", permissions: []);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        LibraryDto request = _libraryDtoFixture.Create(title: "New Books Library", libraryType: "Book");
        request.Id = null;
        _apiFactory.ApiClientStub.RegisterPostResponse("libraries", _libraryDtoFixture.Create(id: Guid.NewGuid(), title: request.Title, libraryType: request.LibraryType));
        HttpRequestMessage saveRequest = CreateSaveRequest(request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(_apiFactory.ApiClientStub.PostRequests, postRequest => postRequest.Endpoint == "libraries");
    }

    [Fact]
    public async Task SaveLibrary_WhenCalledWithInjectionInLibraryTitle_ShouldRemainSecure()
    {
        // Arrange
        ResetApiStub();
        LibraryDto request = _libraryDtoFixture.Create(title: "'; DROP TABLE Libraries--", libraryType: "Book");
        request.Id = null;
        _apiFactory.ApiClientStub.RegisterPostResponse("libraries", _libraryDtoFixture.Create(id: Guid.NewGuid(), title: request.Title, libraryType: request.LibraryType));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content);
    }

    /// <summary>
    /// Resets the stubbed API client and restores the authorization state of an administrator with every permission, because the stub is shared by all the tests of the class.
    /// </summary>
    private void ResetApiStub()
    {
        _apiFactory.ApiClientStub.Reset();
        _apiFactory.ApiClientStub.AuthorizationResponse = _getAuthorizationResponseFixture.Create(
            userId: Guid.NewGuid(),
            role: "Admin",
            permissions: Enum.GetValues<AuthorizationPermission>());
    }

    /// <summary>
    /// Builds the save request that sends the given <paramref name="request"/> to the library save endpoint.
    /// </summary>
    /// <param name="request">The library data to send.</param>
    /// <returns>The configured save request.</returns>
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
