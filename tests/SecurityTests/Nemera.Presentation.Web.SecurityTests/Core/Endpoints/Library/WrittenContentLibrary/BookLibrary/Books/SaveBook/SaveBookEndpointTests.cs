#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Common;
using Nemera.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Common.Exceptions;
using Nemera.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;
using Nemera.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.SecurityTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.SecurityTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;

/// <summary>
/// Contains security tests for the <see cref="SaveBookEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SaveBookEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly UpdateBookRequestFixture _updateBookRequestFixture = new();
    private readonly BookDetailsDtoFixture _bookDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveBookEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public SaveBookEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    public async Task SaveBook_WithSQLInjectionInRouteId_ShouldNormalizeTheIdAndNeverCallTheBodyBookEndpoint(string maliciousBookId)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bodyId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        string normalizedBookId = Guid.Empty.ToString();
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/books/{normalizedBookId}", _bookDetailsDtoFixture.Create());
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(maliciousBookId, _updateBookRequestFixture.Create(id: bodyId, libraryId: libraryId));
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
        Assert.DoesNotContain("at Nemera", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{normalizedBookId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.BookId == normalizedBookId);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{bodyId}");
    }

    [Theory]
    [InlineData("'; DROP TABLE Books; --")] // Destructive injection.
    [InlineData("' OR '1'='1")] // Boolean-based injection.
    public async Task SaveBook_WithSQLInjectionInPostedTitle_ShouldNotLeakOrExecuteIt(string maliciousTitle)
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId, libraryId: libraryId);
        request.Metadata!.Title = maliciousTitle;
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: bookId);
        expectedBook.Metadata!.Title = maliciousTitle;
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/books/{bookId}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(bookId.ToString(), request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{bookId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.Metadata!.Title == maliciousTitle);
    }

    [Fact]
    public async Task SaveBook_WhenLibraryIdIsNotAGuid_ShouldReturnClientErrorWithoutLeakingInternals()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId, libraryId: libraryId);
        string maliciousLibraryId = "'; DROP TABLE Libraries--";
        string payload = JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
        payload = payload.Replace(libraryId.ToString(), maliciousLibraryId);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateRawSaveRequest(bookId.ToString(), payload);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement problemDetails = json.RootElement;
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problemDetails.GetProperty("type").GetString());
        Assert.Equal("General.Validation", problemDetails.GetProperty("title").GetString());
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.GetProperty("status").GetInt32());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails.GetProperty("detail").GetString());
        Assert.Equal($"/en-us/library/written-content-library/books-library/books/{bookId}/api-save-book", problemDetails.GetProperty("instance").GetString());
        Assert.Equal("One or more validation errors occurred", problemDetails.GetProperty("errorMessage").GetString());
        Assert.True(problemDetails.GetProperty("errors").TryGetProperty("libraryId", out JsonElement fieldErrors));
        Assert.Equal("One or more validation errors occurred", fieldErrors[0].GetString());
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Nemera", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(maliciousLibraryId, content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveBook_WhenBodyIsMalformedJson_ShouldReturnClientErrorWithoutLeakingInternals()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        const string PAYLOAD = "{\"contributors\": [";
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateRawSaveRequest(bookId.ToString(), PAYLOAD);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        using JsonDocument json = JsonDocument.Parse(content);
        JsonElement problemDetails = json.RootElement;
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problemDetails.GetProperty("type").GetString());
        Assert.Equal("General.Validation", problemDetails.GetProperty("title").GetString());
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.GetProperty("status").GetInt32());
        Assert.Equal("OneOrMoreValidationErrorsOccurred", problemDetails.GetProperty("detail").GetString());
        Assert.Equal($"/en-us/library/written-content-library/books-library/books/{bookId}/api-save-book", problemDetails.GetProperty("instance").GetString());
        Assert.Equal("One or more validation errors occurred", problemDetails.GetProperty("errorMessage").GetString());
        Assert.NotEmpty(problemDetails.GetProperty("errors").EnumerateObject());
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
        Assert.DoesNotContain(PAYLOAD, content, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Nemera", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveBook_WhenContributorIdIsEmpty_ShouldForwardItAndReturnASanitizedApiError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        string endpoint = $"libraries/{libraryId}/books/{bookId}";
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId, libraryId: libraryId);
        request.Contributors![0].ContributorId = Guid.Empty;
        _apiFactory.ApiClientStub.RegisterPutException(endpoint, new ApiException(
            new ProblemDetailsDto
            {
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Title = "General.Validation",
                Status = 422,
                Detail = "MediaContributorIdCannotBeEmpty"
            },
            (HttpStatusCode)422,
            endpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(bookId.ToString(), request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains("media contributor", json.RootElement.GetProperty("errorMessage").GetString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest =>
            putRequest.Endpoint == endpoint &&
            putRequest.Data is UpdateBookRequest forwardedRequest &&
            forwardedRequest.Contributors![0].ContributorId == Guid.Empty);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Nemera", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveBook_WhenContributorDoesNotExist_ShouldForwardItAndReturnASanitizedApiError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        Guid unknownContributorId = Guid.NewGuid();
        string endpoint = $"libraries/{libraryId}/books/{bookId}";
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId, libraryId: libraryId);
        request.Contributors![0].ContributorId = unknownContributorId;
        _apiFactory.ApiClientStub.RegisterPutException(endpoint, new ApiException(
            new ProblemDetailsDto
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "General.NotFound",
                Status = 404,
                Detail = "MediaContributorNotFound"
            },
            HttpStatusCode.NotFound,
            endpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(bookId.ToString(), request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest =>
            putRequest.Endpoint == endpoint &&
            putRequest.Data is UpdateBookRequest forwardedRequest &&
            forwardedRequest.Contributors![0].ContributorId == unknownContributorId);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at Nemera", content, StringComparison.OrdinalIgnoreCase);
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

    /// <summary>
    /// Builds a save request that sends the given raw JSON body to the book identified by <paramref name="bookId"/>.
    /// </summary>
    /// <param name="bookId">The route Id of the book to save.</param>
    /// <param name="jsonBody">The raw JSON body to send.</param>
    /// <returns>The configured save request.</returns>
    private static HttpRequestMessage CreateRawSaveRequest(string bookId, string jsonBody)
    {
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{Uri.EscapeDataString(bookId)}/api-save-book")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return saveRequest;
    }
}
