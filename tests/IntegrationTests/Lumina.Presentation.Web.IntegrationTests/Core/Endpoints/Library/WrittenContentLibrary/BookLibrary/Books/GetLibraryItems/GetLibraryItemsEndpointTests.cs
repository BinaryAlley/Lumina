#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetLibraryItems;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
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

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetLibraryItems;

/// <summary>
/// Contains integration tests for the <c>/{culture}/library/written-content-library/books-library/books/api-get-library-items</c> route served by the <see cref="GetLibraryItemsEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryItemsEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly PaginatedBookLiteDtoFixture _paginatedBookLiteDtoFixture = new();
    private readonly BookLiteDtoFixture _bookLiteDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryItemsEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public GetLibraryItemsEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task GetLibraryItems_WhenCalledByAuthenticatedUser_ShouldReturnBooksFromApi()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        string searchTerm = "Harry";
        PaginatedBookLiteDto expectedResponse = _paginatedBookLiteDtoFixture.Create(data: [_bookLiteDtoFixture.Create(title: "Harry Potter")]);
        string expectedEndpoint = $"books/lite?libraryId={libraryId}&currentPage=1&perPage=20&searchTerm={searchTerm}&shouldIgnoreThePrefixForAlphaPicker=True";
        _apiFactory.ApiClientStub.RegisterGetResponse(expectedEndpoint, expectedResponse);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);
        HttpRequestMessage getRequest = new(HttpMethod.Get, $"/en-us/library/written-content-library/books-library/books/api-get-library-items?libraryId={libraryId}&currentPage=1&perPage=20&searchTerm={searchTerm}&shouldIgnoreThePrefixForAlphaPicker=true");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        HttpResponseMessage response = await webClient.Client.SendAsync(getRequest);
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(content);
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(expectedResponse.Data.Count, json.RootElement.GetProperty("data").GetProperty("data").GetArrayLength());
        Assert.Contains(expectedEndpoint, _apiFactory.ApiClientStub.GetEndpointsCalled);
    }
}
