#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBook;
using Nemera.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books/{bookId}</c> route.
/// </summary>
public class GetBookEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetBookQuery, Result<BookResponse>> _getBookQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookEndpoint"/> class.
    /// </summary>
    /// <param name="getBookQueryHandler">Injected service for handling get book queries.</param>
    public GetBookEndpoint(IQueryHandler<GetBookQuery, Result<BookResponse>> getBookQueryHandler)
    {
        _getBookQueryHandler = getBookQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_BOOK_BY_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the book identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? bookId = HttpContext.Request.RouteValues["bookId"]?.ToString();
        Result<BookResponse> result = await _getBookQueryHandler.HandleAsync(new GetBookQuery(libraryId, bookId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
