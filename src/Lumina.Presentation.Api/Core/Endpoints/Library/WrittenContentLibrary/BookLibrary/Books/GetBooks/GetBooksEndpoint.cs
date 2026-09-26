#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books</c> route.
/// </summary>
public class GetBooksEndpoint : BaseEndpoint<GetBooksRequest, IResult>
{
    private readonly IQueryHandler<GetBooksQuery, Result<PaginatedResponse<BookResponse>>> _getBooksQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpoint"/> class.
    /// </summary>
    /// <param name="getBooksQueryHandler">Injected service for handling get books queries.</param>
    public GetBooksEndpoint(IQueryHandler<GetBooksQuery, Result<PaginatedResponse<BookResponse>>> getBooksQueryHandler)
    {
        _getBooksQueryHandler = getBooksQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_BOOKS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of all the books of the media library identified by the <c>libraryId</c> route parameter.
    /// </summary>
    /// <param name="request">The request containing the filtering and pagination options of the books to be retrieved.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBooksRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<PaginatedResponse<BookResponse>> result = await _getBooksQueryHandler.HandleAsync(request.ToQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
