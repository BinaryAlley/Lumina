#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Favorites;
using Lumina.Presentation.Web.Common.Requests.Favorites;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.GetFavoriteStatus;

/// <summary>
/// API endpoint for the <c>/{culture}/favorites/api-get-favorite-status</c> route.
/// </summary>
public class GetFavoriteStatusEndpoint : BaseEndpoint<GetFavoriteStatusRequest, IResult>
{
    // The API accepts at most 100 Ids per check, so one more is forwarded to let its validator report an oversized list.
    private const int MAXIMUM_FORWARDED_MEDIA_ITEM_COUNT = 101;

    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetFavoriteStatusEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Favorites.GET_FAVORITE_STATUS);
        DontAutoTag();
        Options(options => options.WithTags("Favorites"));
    }

    /// <summary>
    /// Retrieves which of the given media items are favorites of the current user.
    /// </summary>
    /// <param name="request">The request containing the kind and the Ids of the media items to check.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetFavoriteStatusRequest request, CancellationToken cancellationToken)
    {
        // The favorites belong to the authenticated user, so no intermediary may store them, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The query string is rebuilt from the typed request values instead of forwarding the raw one, so that crafted text can never alter the upstream URL.
        // A missing kind or Id list is left out, so that the API reports the validation error itself.
        List<string> queryParameters = [];
        if (request.MediaKind is not null)
            queryParameters.Add($"mediaKind={Uri.EscapeDataString(request.MediaKind.Value.ToString())}");
        // The forwarded list is bounded, so the upstream URL stays bounded too.
        foreach (Guid mediaItemId in (request.MediaItemIds ?? []).Take(MAXIMUM_FORWARDED_MEDIA_ITEM_COUNT))
            queryParameters.Add($"mediaItemIds={mediaItemId}");
        string url = queryParameters.Count == 0 ? ApiRoutes.Favorites.GET_FAVORITE_STATUS : $"{ApiRoutes.Favorites.GET_FAVORITE_STATUS}?{string.Join('&', queryParameters)}";
        FavoriteStatusDto status = await _apiHttpClient.GetAsync<FavoriteStatusDto>(url, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(status);
    }
}
