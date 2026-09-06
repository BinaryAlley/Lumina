#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.UpdateBook;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.UpdateBook;

/// <summary>
/// API endpoint for the <c>/books/{id}</c> route.
/// </summary>
public class UpdateBookEndpoint : BaseEndpoint<UpdateBookRequest, IResult>
{
    private readonly ICommandHandler<UpdateBookCommand, Result<BookResponse>> _updateBookCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateBookEndpoint"/> class.
    /// </summary>
    /// <param name="updateBookCommandHandler">Injected service for handling update book commands.</param>
    public UpdateBookEndpoint(ICommandHandler<UpdateBookCommand, Result<BookResponse>> updateBookCommandHandler)
    {
        _updateBookCommandHandler = updateBookCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Books.UPDATE_BOOK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the book described in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the edited details of the book.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateBookRequest request, CancellationToken cancellationToken)
    {
        // The book is identified by the {id} route value, not by the id of the request body, so that a mismatched or malicious id can never
        // redirect the update to another book; the raw value is kept as a string, so that an unparseable id reaches the command validator,
        // which reports it with a ProblemDetails validation error instead of failing the request binding.
        request = request with { Id = HttpContext.Request.RouteValues["id"]?.ToString() };

        Result<BookResponse> result = await _updateBookCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
