#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Common.Services;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.Index;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books</c> route.
/// </summary>
public class BooksIndexViewEndpoint : BaseEndpoint<GetBooksViewRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;
    private readonly IUrlService _urlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksIndexViewEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    /// <param name="urlService">Injected service for generating URLs from action and controller names, with localization.</param>
    public BooksIndexViewEndpoint(IApiHttpClient apiHttpClient, IUrlService urlService)
    {
        _apiHttpClient = apiHttpClient;
        _urlService = urlService;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Books.INDEX);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Displays the books browsing view.
    /// </summary>
    /// <param name="request">The request containing the Id of the media library whose books are browsed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBooksViewRequest request, CancellationToken cancellationToken)
    {
        if (request.LibraryId is null)
            return Results.Redirect(_urlService.GetAbsoluteUrl(WebRoutes.Home.INDEX_CULTURED)!);
        LibraryDto library = await _apiHttpClient.GetAsync<LibraryDto>(ApiRoutes.Libraries.GET_LIBRARY_BY_ID.Replace("{id}", request.LibraryId.Value.ToString()), cancellationToken).ConfigureAwait(false);
        return View("/Core/Views/Library/WrittenContentLibrary/BookLibrary/Books/Index.cshtml", library);
    }
}
