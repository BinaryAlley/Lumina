#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.EditBook;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/{id}</c> route.
/// </summary>
public class EditBookViewEndpoint : BaseEndpoint<GetBookRequest, IResult>
{
    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Books.EDIT_BOOK);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Displays the book editing view.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override Task<IResult> ExecuteAsync(GetBookRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(View("/Core/Views/Library/WrittenContentLibrary/BookLibrary/Books/Item.cshtml"));
    }
}
