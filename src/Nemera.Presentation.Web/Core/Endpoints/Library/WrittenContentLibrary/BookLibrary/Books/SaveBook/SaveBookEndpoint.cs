#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.SaveBook;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-save-book</c> route.
/// </summary>
public class SaveBookEndpoint : BaseEndpoint<UpdateBookRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveBookEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public SaveBookEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Books.SAVE_BOOK);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the book described by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the edited details of the book.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateBookRequest request, CancellationToken cancellationToken)
    {
        // The book is identified by the {bookId} route value, not by the id posted in the body, so that a mismatched id can never redirect
        // the update to another book; both ids are normalized to Guids before they are substituted into the upstream URL, so that a crafted
        // value can never escape its URL segment, and an unparseable id is reported by the API as a missing book id.
        string routeId = HttpContext.Request.RouteValues["bookId"]?.ToString() ?? string.Empty;
        string bookId = Guid.TryParse(routeId, out Guid parsedBookId) ? parsedBookId.ToString() : Guid.Empty.ToString();
        UpdateBookRequest requestWithRouteId = request with { BookId = bookId };

        BookDetailsDto book = await _apiHttpClient.PutAsync<BookDetailsDto, UpdateBookRequest>(
            ApiRoutes.Books.UPDATE_BOOK.Replace("{libraryId}", requestWithRouteId.LibraryId.ToString()).Replace("{bookId}", bookId), requestWithRouteId, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(book);
    }
}
