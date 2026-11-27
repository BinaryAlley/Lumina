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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics/api-clear-consumption-history</c> route.
/// </summary>
public class ClearConsumptionHistoryEndpoint : BaseEndpoint<ClearConsumptionHistoryRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearConsumptionHistoryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ClearConsumptionHistoryEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryStatistics.CLEAR_CONSUMPTION_HISTORY);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes every consumption record of the selected user.
    /// </summary>
    /// <param name="request">The request containing the selected user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ClearConsumptionHistoryRequest request, CancellationToken cancellationToken)
    {
        // The history belongs to the selected user, so no intermediary may store any response of this route, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        string path = string.IsNullOrWhiteSpace(request.UserId)
            ? ApiRoutes.LibraryStatistics.CLEAR_CONSUMPTION_HISTORY
            : ApiRoutes.LibraryStatistics.CLEAR_USER_CONSUMPTION_HISTORY.Replace("{userId}", Uri.EscapeDataString(request.UserId), StringComparison.Ordinal);
        await _apiHttpClient.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
