#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeletedBooks;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Lumina.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Fixtures.Common.TestHelpers;
using Lumina.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeletedBooks;

/// <summary>
/// Contains integration tests for the <see cref="DeletedBooksViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedBooksViewEndpointTests : IClassFixture<LuminaWebFactory>
{
    private readonly LuminaWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();
    private readonly DeletedBookDtoFixture _deletedBookDtoFixture = new();
    private readonly ProblemDetailsDtoFixture _problemDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletedBooksViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public DeletedBooksViewEndpointTests(LuminaWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task DeletedBooksView_WhenCalledByAuthenticatedUserWithLibraryId_ShouldRenderDeletedBooksPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto library = _libraryDtoFixture.Create(id: libraryId, title: "Books");
        DeletedBookDto deletedBook = _deletedBookDtoFixture.Create(libraryId: libraryId, title: "Deleted Book");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", library);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/books/deleted-items", new[] { deletedBook });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books/deleted-items?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("Deleted Book", content);
        Assert.Contains("deleted-books-table", content);
        Assert.Contains($"libraries/{libraryId}/books/deleted-items", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task DeletedBooksView_WhenDeletedBookTitleContainsMarkup_ShouldEncodeIt()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto library = _libraryDtoFixture.Create(id: libraryId, title: "Books");
        DeletedBookDto deletedBook = _deletedBookDtoFixture.Create(libraryId: libraryId, title: "<script>alert('x')</script>");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", library);
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}/books/deleted-items", new[] { deletedBook });
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books/deleted-items?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("&lt;script&gt;alert", content);
        Assert.DoesNotContain("<script>alert('x')</script>", content);
    }

    [Fact]
    public async Task DeletedBooksView_WhenTheApiReturnsForbidden_ShouldRedirectWithoutLeakingDetails()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto library = _libraryDtoFixture.Create(id: libraryId, title: "Books");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", library);
        string endpoint = $"libraries/{libraryId}/books/deleted-items";
        _apiFactory.ApiClientStub.RegisterGetException(endpoint, new ApiException(
            _problemDetailsDtoFixture.Create(title: "General.Unauthorized", detail: "NotAuthorized"),
            HttpStatusCode.Forbidden,
            endpoint));
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books/deleted-items?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        // A browser navigation that the API forbids is redirected to the access denied page, without rendering any of the forbidden data.
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqliteException", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeletedBooksView_WhenCalledWithoutLibraryId_ShouldRedirectToHome()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/library/written-content-library/books-library/books/deleted-items");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/en-us", response.Headers.Location!.ToString());
    }
}
