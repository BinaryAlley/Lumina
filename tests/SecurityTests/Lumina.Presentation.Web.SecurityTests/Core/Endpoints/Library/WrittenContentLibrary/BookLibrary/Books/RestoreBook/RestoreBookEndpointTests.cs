#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;

/// <summary>
/// Contains security tests for the <see cref="RestoreBookEndpoint"/> class.
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

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task RestoreBook_WithSQLInjectionInRouteId_ShouldNormalizeTheIdAndNeverRestoreAnotherBook(string maliciousId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPostResponse<object>($"libraries/{libraryId}/books/{normalizedId}/restore", new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest(maliciousId, libraryId.ToString());
        restoreRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"libraries/{libraryId}/books/{normalizedId}/restore", _apiFactory.ApiClientStub.PostRequests.Select(request => request.Endpoint));
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task RestoreBook_WithSQLInjectionInLibraryQueryId_ShouldNormalizeTheLibraryId(string maliciousLibraryId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        string normalizedId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPostResponse<object>($"libraries/{normalizedId}/books/{bookId}/restore", new Lumina.Presentation.Web.Common.Requests.Common.EmptyRequest());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest(bookId.ToString(), maliciousLibraryId);
        restoreRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"libraries/{normalizedId}/books/{bookId}/restore", _apiFactory.ApiClientStub.PostRequests.Select(request => request.Endpoint));
    }

    [Fact]
    public async Task RestoreBook_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequestAndNotCallApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        _apiFactory.ApiClientStub.Reset();
        HttpRequestMessage restoreRequest = CreateRestoreRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PostRequests);
    }

    [Fact]
    public async Task RestoreBook_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage restoreRequest = CreateRestoreRequest(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(restoreRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PostRequests);
    }

    /// <summary>
    /// Builds the restore request for the book identified by the route, with the owning library passed through the query string.
    /// </summary>
    /// <param name="bookId">The raw route value of the book Id.</param>
    /// <param name="libraryId">The raw query value of the library Id.</param>
    /// <returns>The configured restore request.</returns>
    private static HttpRequestMessage CreateRestoreRequest(string bookId, string libraryId)
    {
        HttpRequestMessage restoreRequest = new(HttpMethod.Post, $"/en-us/library/written-content-library/books-library/books/{Uri.EscapeDataString(bookId)}/api-restore-book?libraryId={Uri.EscapeDataString(libraryId)}")
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
