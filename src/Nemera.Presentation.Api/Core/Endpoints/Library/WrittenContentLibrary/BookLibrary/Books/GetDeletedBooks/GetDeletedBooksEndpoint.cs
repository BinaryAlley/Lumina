#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Queries.GetDeletedBooks;
using Nemera.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetDeletedBooks;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books/deleted-items</c> route.
/// </summary>
public class GetDeletedBooksEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetDeletedBooksQuery, Result<IReadOnlyList<DeletedBookResponse>>> _getDeletedBooksQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDeletedBooksEndpoint"/> class.
    /// </summary>
    /// <param name="getDeletedBooksQueryHandler">Injected service for handling get deleted books queries.</param>
    public GetDeletedBooksEndpoint(IQueryHandler<GetDeletedBooksQuery, Result<IReadOnlyList<DeletedBookResponse>>> getDeletedBooksQueryHandler)
    {
        _getDeletedBooksQueryHandler = getDeletedBooksQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_DELETED_BOOKS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the deleted books of the library identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier of the library from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        Result<IReadOnlyList<DeletedBookResponse>> result = await _getDeletedBooksQueryHandler.HandleAsync(new GetDeletedBooksQuery(libraryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
