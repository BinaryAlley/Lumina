#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;

/// <summary>
/// Contains integration tests for the <see cref="GetBookDetailsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookDetailsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly BookDetailsDtoFixture _bookDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookDetailsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetBookDetailsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetBookDetails_WhenCalledByAuthenticatedUser_ShouldReturnBookDetailsFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid bookId = Guid.NewGuid();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create(id: bookId);
        _apiFactory.ApiClientStub.RegisterGetResponse($"books/{bookId}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/written-content-library/books-library/books/{bookId}/api-get-book");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(bookId, json.RootElement.GetProperty("data").GetProperty("id").GetGuid());
        Assert.Contains($"books/{bookId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task GetBookDetails_WhenNotAuthenticated_ShouldRedirectToLogin()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        HttpClient anonymousClient = WebTestHelpers.CreateAnonymousClient(_apiFactory);

        // Act
        HttpResponseMessage response = await anonymousClient.GetAsync($"/en-us/library/written-content-library/books-library/books/{Guid.NewGuid()}/api-get-book");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(_apiFactory.ApiClientStub.GetEndpointsCalled, endpoint => endpoint.StartsWith("books/", StringComparison.OrdinalIgnoreCase));
    }
}
