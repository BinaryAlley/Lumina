#region ========================================================================= USING =====================================================================================
using ErrorOr;
using FastEndpoints;
using Lumina.Application.Common.Mapping.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Requests.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Core.Aggregates.WrittenContentLibrary.BookLibraryAggregate;
using Lumina.Presentation.Api.Common.Routes.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Mediator;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.WrittenContentLibrary.BookLibrary.GetBook;

/// <summary>
/// API endpoint for the <c>/books</c> route.
/// </summary>
public class GetBookEndpoint : BaseEndpoint<GetBookRequest, IResult>
{
    private readonly ISender _sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBookEndpoint"/> class.
    /// </summary>
    /// <param name="sender">Injected service for mediating commands and queries.</param>
    public GetBookEndpoint(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Books.GET_BOOK_BY_ID);
        Version(1);
        AllowAnonymous();
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds a book stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the book to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBookRequest request, CancellationToken cancellationToken)
    {
        //ErrorOr<Book> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        //return result.Match(success => TypedResults.CreatedAtRoute($"/api/v1/books/{success.Id}", success), Problem);
        return await Task.FromResult(TypedResults.Ok());
    }
}
