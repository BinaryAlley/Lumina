#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;

/// <summary>
/// Contains integration tests for the <see cref="RestoreBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreBookEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public RestoreBookEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task RestoreBook_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardPostAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        _apiFactory.ApiClientStub.RegisterPostSuccess($"libraries/{libraryId}/books/{bookId}/restore");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest(bookId, libraryId);
        restoreRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("data").GetProperty("isRestored").GetBoolean());
        Assert.Contains($"libraries/{libraryId}/books/{bookId}/restore", _apiFactory.ApiClientStub.PostRequests.Select(request => request.Endpoint));
    }

    [Fact]
    public async Task RestoreBook_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        Guid bookId = Guid.NewGuid();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        _apiFactory.ApiClientStub.Reset();
        HttpRequestMessage restoreRequest = CreateRestoreRequest(bookId, libraryId);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PostRequests);
    }

    [Fact]
    public async Task RestoreBook_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest(Guid.NewGuid(), Guid.NewGuid());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PostRequests);
    }

    /// <summary>
    /// Builds the restore request for the book identified by the route, with the owning library passed through the query string.
    /// </summary>
    /// <param name="bookId">The route Id of the book to restore.</param>
    /// <param name="libraryId">The Id of the media library the book belongs to.</param>
    /// <returns>The configured restore request.</returns>
    private static HttpRequestMessage CreateRestoreRequest(Guid bookId, Guid libraryId)
    {
        HttpRequestMessage restoreRequest = new(HttpMethod.Post, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-restore-book?libraryId={libraryId}")
        {
            // The front-end sends the JSON content type even for bodyless POST requests, which triggers the antiforgery validation.
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        // The antiforgery middleware matches the content type exactly, so the charset suffix must be omitted.
        restoreRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        restoreRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return restoreRequest;
    }
}
