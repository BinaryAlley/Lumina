#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BooksLibrary.Books.Commands.AddBook;
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.AddBook;

/// <summary>
/// API endpoint for the <c>/books</c> route.
/// </summary>
public class AddBookEndpoint : BaseEndpoint<AddBookRequest, IResult>
{
    private readonly ICommandHandler<AddBookCommand, ErrorOr<BookResponse>> _addBookCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddBookEndpoint"/> class.
    /// </summary>
    /// <param name="addBookCommandHandler">Injected service for handling add book commands.</param>
    public AddBookEndpoint(ICommandHandler<AddBookCommand, ErrorOr<BookResponse>> addBookCommandHandler)
    {
        _addBookCommandHandler = addBookCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Books.ADD_BOOK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds a book stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the book to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddBookRequest request, CancellationToken cancellationToken)
    {
        ErrorOr<BookResponse> result = await _addBookCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1{ApiRoutes.Books.ADD_BOOK}/{result.Value.Id}", result.Value), Problem);
    }
}
