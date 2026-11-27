#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeleteBook;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{bookId}/api-delete-book</c> route.
/// </summary>
public class DeleteBookEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DeleteBookEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.DELETE);
        Routes(WebRoutes.Books.DELETE_BOOK);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes the book identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The book is identified by the {bookId} route value and the owning library by the {libraryId} query string; both ids are
        // normalized to Guids before they are substituted into the upstream URL, so that a crafted value can never escape its URL
        // segment, and an unparseable id is reported by the API as a missing id.
        string routeBookId = HttpContext.Request.RouteValues["bookId"]?.ToString() ?? string.Empty;
        string bookId = Guid.TryParse(routeBookId, out Guid parsedBookId) ? parsedBookId.ToString() : Guid.Empty.ToString();
        string queryLibraryId = HttpContext.Request.Query["libraryId"].ToString();
        string libraryId = Guid.TryParse(queryLibraryId, out Guid parsedLibraryId) ? parsedLibraryId.ToString() : Guid.Empty.ToString();

        await _apiHttpClient.DeleteAsync(ApiRoutes.Books.DELETE_BOOK.Replace("{libraryId}", libraryId).Replace("{bookId}", bookId), cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
