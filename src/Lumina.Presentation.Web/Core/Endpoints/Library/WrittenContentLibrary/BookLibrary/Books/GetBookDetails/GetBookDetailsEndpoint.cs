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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBookDetails;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{id}/api-get-book</c> route.
/// </summary>
public class GetBookDetailsEndpoint : BaseEndpoint<GetBookRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookDetailsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetBookDetailsEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Books.GET_BOOK_DETAILS);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Retrieves the full details of the book identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the book to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookRequest request, CancellationToken cancellationToken)
    {
        // The route value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted route value can
        // never escape its URL segment, and an unparseable id is reported by the API as a missing book id.
        request = request with { Id = Guid.TryParse(request.Id, out Guid bookId) ? bookId.ToString() : Guid.Empty.ToString() };

        BookDetailsDto book = await _apiHttpClient.GetAsync<BookDetailsDto>(ApiRoutes.Books.GET_BOOK.Replace("{id}", request.Id), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(book);
    }
}
