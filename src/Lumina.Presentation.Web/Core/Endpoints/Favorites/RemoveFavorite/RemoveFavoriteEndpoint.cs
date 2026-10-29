#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Enums.Favorites;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Favorites.RemoveFavorite;

/// <summary>
/// API endpoint for the <c>/{culture}/favorites/{mediaKind}/{mediaItemId}/api-remove-favorite</c> route.
/// </summary>
public class RemoveFavoriteEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    // A value that is not a name of FavoriteMediaKind, so the API rejects a crafted route value with its own validation error.
    private const string INVALID_MEDIA_KIND_PLACEHOLDER = "Invalid";

    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public RemoveFavoriteEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.DELETE);
        Routes(WebRoutes.Favorites.REMOVE_FAVORITE);
        DontAutoTag();
        Options(options => options.WithTags("Favorites"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Removes a media item from the favorites of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The route values are normalized before they are substituted into the upstream URL, so that a crafted value can never escape its URL segment.
        // The media kind must be the name of a FavoriteMediaKind member, which also rejects numeric strings and comma separated flag lists, and the Id must be a Guid.
        string routeMediaKind = HttpContext.Request.RouteValues["mediaKind"]?.ToString() ?? string.Empty;
        string mediaKind = INVALID_MEDIA_KIND_PLACEHOLDER;
        foreach (string mediaKindName in Enum.GetNames<FavoriteMediaKind>())
            if (mediaKindName != nameof(FavoriteMediaKind.None) && string.Equals(mediaKindName, routeMediaKind, StringComparison.OrdinalIgnoreCase))
                mediaKind = mediaKindName;
        string routeMediaItemId = HttpContext.Request.RouteValues["mediaItemId"]?.ToString() ?? string.Empty;
        string mediaItemId = Guid.TryParse(routeMediaItemId, out Guid parsedMediaItemId) ? parsedMediaItemId.ToString() : Guid.Empty.ToString();
        await _apiHttpClient.DeleteAsync(
            ApiRoutes.Favorites.REMOVE_FAVORITE
                .Replace("{mediaKind}", mediaKind)
                .Replace("{mediaItemId}", mediaItemId),
            cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
