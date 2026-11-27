#region ========================================================================= USING =====================================================================================
using Nemera.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.EditBook;
using Nemera.Presentation.Web.Fixtures.Common.DTO.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Web.Fixtures.Common.TestHelpers;
using Nemera.Presentation.Web.IntegrationTests.Common.Setup;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.IntegrationTests.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.EditBook;

/// <summary>
/// Contains integration tests for the <see cref="EditBookViewEndpoint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditBookViewEndpointTests : IClassFixture<NemeraWebFactory>
{
    private readonly NemeraWebFactory _apiFactory;
    private readonly BookDetailsDtoFixture _bookDetailsDtoFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EditBookViewEndpointTests"/> class.
    /// </summary>
    /// <param name="apiFactory">Injected Web application factory.</param>
    public EditBookViewEndpointTests(NemeraWebFactory apiFactory)
    {
        _apiFactory = apiFactory;
    }

    [Fact]
    public async Task EditBook_WhenCalledByAuthenticatedUser_ShouldRenderEditBookPage()
    {
        // Arrange
        _apiFactory.ApiClientStub.Reset();
        BookDetailsDto expectedBook = _bookDetailsDtoFixture.Create();
        expectedBook.Metadata!.Title = "A book";
        _apiFactory.ApiClientStub.RegisterGetResponse($"libraries/{expectedBook.LibraryId}/books/{expectedBook.Id}", expectedBook);
        AuthenticatedWebClient webClient = await WebTestHelpers.CreateAuthenticatedClientAsync(_apiFactory);

        // Act
        HttpResponseMessage response = await webClient.Client.GetAsync($"/en-us/library/written-content-library/books-library/books/{expectedBook.Id}?libraryId={expectedBook.LibraryId}");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("A book", content);
        Assert.Contains("btn-delete-book", content);
    }
}
