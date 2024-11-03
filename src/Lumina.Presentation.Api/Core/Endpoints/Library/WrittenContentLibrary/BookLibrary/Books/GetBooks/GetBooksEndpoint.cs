#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Queries.GetBooks;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBooks;

/// <summary>
/// API endpoint for the <c>/books</c> route.
/// </summary>
public class GetBooksEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBooksEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetBooksEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_BOOKS);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the list of all books.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest _, CancellationToken cancellationToken)
    {
        ErrorOr<IEnumerable<BookResponse>> result = await _sender.Send(new GetBooksQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
