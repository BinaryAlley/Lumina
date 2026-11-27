#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;
using Nemera.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;

/// <summary>
/// Contains integration tests for the <see cref="SaveBookEndpoint"/> class.
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

    [Fact]
    public async Task SaveBook_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardUpdateAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId, libraryId: libraryId);
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: bookId);
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/books/{bookId}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(bookId, request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(bookId, json.RootElement.GetProperty("data").GetProperty("id").GetGuid());
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{bookId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.BookId == bookId.ToString());
    }

    [Fact]
    public async Task SaveBook_WhenRouteIdDiffersFromBodyId_ShouldForwardTheRouteIdToApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid routeId = Guid.NewGuid();
        Guid bodyId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bodyId, libraryId: libraryId);
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: routeId);
        _apiFactory.ApiClientStub.RegisterPutResponse($"libraries/{libraryId}/books/{routeId}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(routeId, request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{routeId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.BookId == routeId.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{libraryId}/books/{bodyId}");
    }

    [Fact]
    public async Task SaveBook_WhenCalledWithoutAntiforgeryToken_ShouldRejectRequest()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(bookId, request);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"libraries/{request.LibraryId}/books/{bookId}");
    }

    [Fact]
    public async Task SaveBook_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(Guid.NewGuid(), _updateBookRequestFixture.Create());

        // Act
        HttpResponseMessage response = await anonymousClient.SendAsync(saveRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(_apiFactory.ApiClientStub.PutRequests);
    }

    [Fact]
    public async Task SaveBook_WhenLibraryIdIsNotAGuid_ShouldReturnClientError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        Guid libraryId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId, libraryId: libraryId);
        string payload = JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
        payload = payload.Replace(libraryId.ToString(), "not-a-guid");
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-save-book")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
    }

    [Fact]
    public async Task SaveBook_WhenBodyIsMalformedJson_ShouldReturnClientError()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        const string PAYLOAD = "{\"contributors\": [";
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-save-book")
        {
            Content = new StringContent(PAYLOAD, Encoding.UTF8, "application/json")
        };
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
    }

    /// <summary>
    /// Builds the save request that PUTs the given <paramref name="request"/> to the book identified by <paramref name="bookId"/>.
    /// </summary>
    /// <param name="bookId">The route Id of the book to save.</param>
    /// <param name="request">The book data to send.</param>
    /// <returns>The configured save request.</returns>
    private static HttpRequestMessage CreateSaveRequest(Guid bookId, UpdateBookRequest request)
    {
        HttpRequestMessage saveRequest = new(HttpMethod.Put, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-save-book")
        {
            Content = JsonContent.Create(request)
        };
        saveRequest.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        saveRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return saveRequest;
    }
}
