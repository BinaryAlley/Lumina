#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Commands.RestoreBook;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.RestoreBook;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/books/{bookId}/restore</c> route.
/// </summary>
public class RestoreBookEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<RestoreBookCommand, Result<Updated>> _restoreBookCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreBookEndpoint"/> class.
    /// </summary>
    /// <param name="restoreBookCommandHandler">Injected service for handling restore book commands.</param>
    public RestoreBookEndpoint(ICommandHandler<RestoreBookCommand, Result<Updated>> restoreBookCommandHandler)
    {
        _restoreBookCommandHandler = restoreBookCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Books.RESTORE_BOOK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Restores the deleted book identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? bookId = HttpContext.Request.RouteValues["bookId"]?.ToString();
        Result<Updated> result = await _restoreBookCommandHandler.HandleAsync(new RestoreBookCommand(libraryId, bookId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
