#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.DeleteBook;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.DeleteBook;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books/{bookId}</c> route.
/// </summary>
public class DeleteBookEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<DeleteBookCommand, Result<Deleted>> _deleteBookCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteBookEndpoint"/> class.
    /// </summary>
    /// <param name="deleteBookCommandHandler">Injected service for handling delete book commands.</param>
    public DeleteBookEndpoint(ICommandHandler<DeleteBookCommand, Result<Deleted>> deleteBookCommandHandler)
    {
        _deleteBookCommandHandler = deleteBookCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Books.DELETE_BOOK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes a book by Id.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? bookId = HttpContext.Request.RouteValues["bookId"]?.ToString();
        Result<Deleted> result = await _deleteBookCommandHandler.HandleAsync(new DeleteBookCommand(libraryId, bookId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
