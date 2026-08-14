#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Api.Common.Routes.Library.WrittenContentLibrary.BookLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetBook;

/// <summary>
/// API endpoint for the <c>/books/{id}</c> route.
/// </summary>
public class GetBookEndpoint : BaseEndpoint<GetBookRequest, IResult>
{
    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
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
        //Result<Book> result = await _sender.Send(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        //return result.Match(success => TypedResults.CreatedAtRoute($"/api/v1/books/{success.Id}", success), Problem);
        return await Task.FromResult(TypedResults.Ok());
    }
}
