#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Models.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Controllers.Library.WrittenContentLibrary.BookLibrary;

/// <summary>
/// Controller for books related operations.
/// </summary>
[Authorize]
[Route("{culture}/library/written-content-library/books-library/books")]
public class BooksController : Controller
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksController"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public BooksController(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Displays the view for browsing the books of a media library.
    /// </summary>
    /// <param name="libraryId">The Id of the media library whose books are browsed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? libraryId, CancellationToken cancellationToken = default)
    {
        if (libraryId is null)
            return RedirectToAction("Index", "Home");

        LibraryModel library = await _apiHttpClient.GetAsync<LibraryModel>($"libraries/{libraryId}", cancellationToken).ConfigureAwait(false);
        return View("/Views/Library/WrittenContentLibrary/BookLibrary/Books/Index.cshtml", library);
    }

    /// <summary>
    /// Gets the lightweight details of the books of a media library.
    /// </summary>
    /// <param name="query">The model containing the parameters used to retrieve the books.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    [HttpGet("api-get-library-items")]
    public async Task<IActionResult> GetLibraryItems([FromQuery] BookLibraryItemsQueryModel query, CancellationToken cancellationToken = default)
    {
        PaginatedBookLiteModel response = await _apiHttpClient.GetAsync<PaginatedBookLiteModel>(BuildItemsEndpoint(query), cancellationToken).ConfigureAwait(false);
        return Json(new { success = true, data = response });
    }

    /// <summary>
    /// Displays the view for a single book.
    /// </summary>
    /// <param name="id">The Id of the book to display.</param>
    [HttpGet("{id}")]
    public IActionResult EditBook(Guid id)
    {
        return View("/Views/Library/WrittenContentLibrary/BookLibrary/Books/Item.cshtml");
    }

    /// <summary>
    /// Builds the API endpoint used to retrieve the lightweight details of the books of a media library, from the provided <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The model containing the parameters used to retrieve the books.</param>
    /// <returns>The API endpoint to which the retrieval request is sent.</returns>
    private static string BuildItemsEndpoint(BookLibraryItemsQueryModel query)
    {
        StringBuilder endpoint = new($"books/lite?libraryId={query.LibraryId}");
        if (query.CurrentPage is not null)
            endpoint.Append($"&currentPage={query.CurrentPage}");
        if (query.PerPage is not null)
            endpoint.Append($"&perPage={query.PerPage}");
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            endpoint.Append($"&searchTerm={Uri.EscapeDataString(query.SearchTerm)}");
        if (!string.IsNullOrWhiteSpace(query.FilterAlphaKey))
            endpoint.Append($"&filterAlphaKey={Uri.EscapeDataString(query.FilterAlphaKey)}");
        endpoint.Append($"&ignoreThePrefixForAlphaPicker={query.IgnoreThePrefixForAlphaPicker}");
        if (!string.IsNullOrWhiteSpace(query.SortBy))
            endpoint.Append($"&sortBy={Uri.EscapeDataString(query.SortBy)}");
        if (query.SortOrder is not null)
            endpoint.Append($"&sortOrder={query.SortOrder}");
        return endpoint.ToString();
    }
}
