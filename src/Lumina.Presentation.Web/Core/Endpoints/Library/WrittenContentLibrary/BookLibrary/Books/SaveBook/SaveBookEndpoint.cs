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
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{id}/api-save-book</c> route.
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
        // The book is identified by the {id} route value, not by the id posted in the form, so that a mismatched id can never redirect
        // the update to another book; the value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted
        // route value can never escape its URL segment, and an unparseable id is reported by the API as a missing book id.
        string routeId = HttpContext.Request.RouteValues["id"]?.ToString() ?? string.Empty;
        request.Id = Guid.TryParse(routeId, out Guid bookId) ? bookId.ToString() : Guid.Empty.ToString();

        BookDetailsDto book = await _apiHttpClient.PutAsync<BookDetailsDto, UpdateBookRequest>(
            ApiRoutes.Books.UPDATE_BOOK.Replace("{id}", request.Id), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(book);
    }
}
