#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavorites;

/// <summary>
/// API endpoint for the <c>/{culture}/favorites/api-get-favorites</c> route.
/// </summary>
public class GetFavoritesEndpoint : BaseEndpoint<GetFavoritesRequest, IResult>
{
    private const int DEFAULT_FAVORITE_PAGE_SIZE = 200;

    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetFavoritesEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Favorites.GET_FAVORITES);
        DontAutoTag();
        Options(options => options.WithTags("Favorites"));
    }

    /// <summary>
    /// Retrieves a page of the favorites of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetFavoritesRequest request, CancellationToken cancellationToken)
    {
        // The favorites belong to the authenticated user, so no intermediary may store them, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The query string is rebuilt from the typed request values instead of forwarding the raw one, so that crafted text can never alter the upstream URL.
        // The page bounds are normalized by the API pagination data, exactly like every other paginated collection of the application.
        int currentPage = request.CurrentPage ?? 1;
        int perPage = request.PerPage ?? DEFAULT_FAVORITE_PAGE_SIZE;
        string url = ApiRoutes.Favorites.GET_FAVORITES + $"?currentPage={currentPage.ToString(CultureInfo.InvariantCulture)}&perPage={perPage.ToString(CultureInfo.InvariantCulture)}";
        if (request.MediaKind is not null)
            url += $"&mediaKind={Uri.EscapeDataString(request.MediaKind.Value.ToString())}";
        PaginatedDto<FavoriteItemDto> favorites = await _apiHttpClient.GetAsync<PaginatedDto<FavoriteItemDto>>(url, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(favorites);
    }
}
