#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.DeleteConsumptionItemHistory;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics/api-delete-consumption-item-history</c> route.
/// </summary>
public class DeleteConsumptionItemHistoryEndpoint : BaseEndpoint<DeleteConsumptionItemHistoryRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionItemHistoryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DeleteConsumptionItemHistoryEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryStatistics.DELETE_CONSUMPTION_ITEM_HISTORY);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes every consumption record of a media item of the selected user.
    /// </summary>
    /// <param name="request">The request containing the selected user, the media item, and its library.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteConsumptionItemHistoryRequest request, CancellationToken cancellationToken)
    {
        // The records belong to the selected user, so no intermediary may store any response of this route, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        string path = string.IsNullOrWhiteSpace(request.UserId)
            ? ApiRoutes.LibraryStatistics.DELETE_CONSUMPTION_ITEM_HISTORY
            : ApiRoutes.LibraryStatistics.DELETE_USER_CONSUMPTION_ITEM_HISTORY.Replace("{userId}", Uri.EscapeDataString(request.UserId), StringComparison.Ordinal);
        path = path
            .Replace("{mediaKind}", request.MediaKind.ToString(), StringComparison.Ordinal)
            .Replace("{mediaItemId}", request.MediaItemId.ToString(), StringComparison.Ordinal)
            .Replace("{libraryId}", request.LibraryId.ToString(), StringComparison.Ordinal);
        await _apiHttpClient.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
