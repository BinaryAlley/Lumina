#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using Lumina.Presentation.Web.Common.Requests.Libraries;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.WrittenContentLibrary.BookLibrary.Books.GetLibraryItems;

/// <summary>
/// API endpoint for the <c>/{culture}/library/written-content-library/books-library/books/api-get-library-items</c> route.
/// </summary>
public class GetLibraryItemsEndpoint : BaseEndpoint<GetBooksLiteRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetLibraryItemsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetLibraryItemsEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Books.GET_LIBRARY_ITEMS);
        DontAutoTag();
        Options(options => options.WithTags("Books"));
    }

    /// <summary>
    /// Retrieves the lightweight details of the books of a media library.
    /// </summary>
    /// <param name="request">The request containing the parameters used to retrieve the books.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetBooksLiteRequest request, CancellationToken cancellationToken)
    {
        PaginatedBookLiteDto response = await _apiHttpClient.GetAsync<PaginatedBookLiteDto>(BuildItemsEndpoint(request), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }

    /// <summary>
    /// Builds the API endpoint used to retrieve the lightweight details of the books of a media library, from the provided <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The request containing the parameters used to retrieve the books.</param>
    /// <returns>The API endpoint to which the retrieval request is sent.</returns>
    private static string BuildItemsEndpoint(GetBooksLiteRequest query)
    {
        StringBuilder endpoint = new(ApiRoutes.Books.GET_BOOKS_LITE);
        endpoint.Append($"?libraryId={query.LibraryId}");
        if (query.CurrentPage is not null)
            endpoint.Append($"&currentPage={query.CurrentPage}");
        if (query.PerPage is not null)
            endpoint.Append($"&perPage={query.PerPage}");
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            endpoint.Append($"&searchTerm={Uri.EscapeDataString(query.SearchTerm)}");
        if (!string.IsNullOrWhiteSpace(query.FilterAlphaKey))
            endpoint.Append($"&filterAlphaKey={Uri.EscapeDataString(query.FilterAlphaKey)}");
        endpoint.Append($"&shouldIgnoreThePrefixForAlphaPicker={query.ShouldIgnoreThePrefixForAlphaPicker}");
        if (!string.IsNullOrWhiteSpace(query.SortBy))
            endpoint.Append($"&sortBy={Uri.EscapeDataString(query.SortBy)}");
        if (query.SortOrder is not null)
            endpoint.Append($"&sortOrder={query.SortOrder}");
        return endpoint.ToString();
    }
}
