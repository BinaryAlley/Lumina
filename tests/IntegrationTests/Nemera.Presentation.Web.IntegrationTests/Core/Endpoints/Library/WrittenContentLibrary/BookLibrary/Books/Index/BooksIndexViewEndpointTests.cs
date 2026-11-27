#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.Libraries;
using Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;
using Nemera.Presentation.Web.Fixtures.Common.DTO.Libraries;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;

/// <summary>
/// Contains integration tests for the <see cref="BooksIndexViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BooksIndexViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly LibraryDtoFixture _libraryDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksIndexViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public BooksIndexViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task BooksIndex_WhenCalledByAuthenticatedUserWithLibraryId_ShouldRenderBooksPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto expectedLibrary = _libraryDtoFixture.Create(id: libraryId, title: "Books");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", expectedLibrary);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("search-input", content);
        Assert.Contains("card-action-delete", content);
        Assert.Contains($"libraries/{libraryId}", _apiFactory.ApiClientStub.GetEndpointsCalled);
    }

    [Fact]
    public async Task BooksIndex_WhenTheCultureUsesApostrophes_ShouldSerializeTheLocalizedStringsAsJson()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        Guid libraryId = Guid.NewGuid();
        LibraryDto expectedLibrary = _libraryDtoFixture.Create(id: libraryId, title: "Livres");
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{libraryId}", expectedLibrary);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/fr-fr/library/written-content-library/books-library/books?libraryId={libraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        // The apostrophes and accents are JSON-escaped, so the translations stay valid JavaScript string literals.
        Assert.Contains("deleteFailed: \"L", content);
        Assert.Contains("deleteConfirm: \"", content);
        Assert.DoesNotContain("deleteFailed: '", content);
        Assert.DoesNotContain("deleteConfirm: '", content);
    }

    [Fact]
    public async Task BooksIndex_WhenCalledByAuthenticatedUserWithoutLibraryId_ShouldRedirectToHome()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync("/en-us/library/written-content-library/books-library/books");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/en-us", response.Headers.Location!.ToString());
    }
}
