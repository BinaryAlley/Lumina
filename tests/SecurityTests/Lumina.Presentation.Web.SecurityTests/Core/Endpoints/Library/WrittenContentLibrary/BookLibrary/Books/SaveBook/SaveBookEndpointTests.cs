#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.SecurityTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;

/// <summary>
/// Contains security tests for the <see cref="SaveBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveBookEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly UpdateBookRequestFixture _updateBookRequestFixture = new();
    private readonly BookDetailsDtoFixture _bookDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SaveBookEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveBook_WithSQLInjectionInRouteId_ShouldNormalizeTheIdAndNeverCallTheBodyBookEndpoint(string maliciousBookId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bodyId = Guid.NewGuid();
        string normalizedBookId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{normalizedBookId}", _bookDetailsDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(maliciousBookId, _updateBookRequestFixture.Create(id: bodyId));
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // The route is authoritative and its value is normalized to a Guid before it is substituted into the upstream URL, so a crafted
        // route value can never escape its URL segment or redirect the update to the book carried in the body, and the response must not
        // leak stack traces or database details.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Lumina", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{normalizedBookId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.Id == normalizedBookId);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bodyId}");
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // destructive injection
    [InlineData("' OR '1'='1")] // boolean-based injection
    public async Task SaveBook_WithSQLInjectionInPostedTitle_ShouldNotLeakOrExecuteIt(string maliciousTitle)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId);
        request.Metadata!.Title = maliciousTitle;
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: bookId);
        expectedBook.Metadata!.Title = maliciousTitle;
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{bookId}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(bookId.ToString(), request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("SQL", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bookId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.Metadata!.Title == maliciousTitle);
    }

    [Fact]
    public async Task SaveBook_WhenCalledWithoutAuthentication_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(Guid.NewGuid().ToString(), _updateBookRequestFixture.Create());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    /// <summary>
    /// Builds the save request that PUTs the given <paramref name="request"/> to the book identified by <paramref name="bookId"/>.
    /// </summary>
    /// <param name="bookId">The route Id of the book to save.</param>
    /// <param name="request">The book data to send.</param>
    /// <returns>The configured save request.</returns>
    private static HttpRequestMessage CreateSaveRequest(string bookId, UpdateBookRequest request)
    {
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{Uri.EscapeDataString(bookId)}/api-save-book")
        {
            Content = JsonContent.Create(request)
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return saveRequest;
    }
}
