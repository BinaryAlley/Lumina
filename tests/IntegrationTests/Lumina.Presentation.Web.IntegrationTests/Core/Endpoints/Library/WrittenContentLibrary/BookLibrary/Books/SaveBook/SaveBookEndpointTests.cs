#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;

/// <summary>
/// Contains integration tests for the <see cref="SaveBookEndpoint"/> class.
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

    [Fact]
    public async Task SaveBook_WhenCalledByAuthenticatedUserWithAntiforgeryToken_ShouldForwardUpdateAndReturnSuccess()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bookId);
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: bookId);
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{bookId}", expectedBook);
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
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bookId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.Id == bookId.ToString());
    }

    [Fact]
    public async Task SaveBook_WhenRouteIdDiffersFromBodyId_ShouldForwardTheRouteIdToApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid routeId = Guid.NewGuid();
        Guid bodyId = Guid.NewGuid();
        UpdateBookRequest request = _updateBookRequestFixture.Create(id: bodyId);
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: routeId);
        _apiFactory.ApiClientStub.RegisterPutResponse($"books/{routeId}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage saveRequest = CreateSaveRequest(routeId, request);
        saveRequest.Headers.Add("RequestVerificationToken", webClient.AntiforgeryToken);

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(saveRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{routeId}" && putRequest.Data is UpdateBookRequest forwarded && forwarded.Id == routeId.ToString());
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bodyId}");
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
        Assert.DoesNotContain(_apiFactory.ApiClientStub.PutRequests, putRequest => putRequest.Endpoint == $"books/{bookId}");
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
