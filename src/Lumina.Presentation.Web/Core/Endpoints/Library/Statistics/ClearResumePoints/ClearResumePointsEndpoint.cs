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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.ClearResumePoints;

/// <summary>
/// API endpoint for the <c>/{culture}/library/statistics/api-clear-resume-points</c> route.
/// </summary>
public class ClearResumePointsEndpoint : BaseEndpoint<ClearResumePointsRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public ClearResumePointsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.LibraryStatistics.CLEAR_RESUME_POINTS);
        DontAutoTag();
        Options(options => options.WithTags("Statistics"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes the resume points of the selected user.
    /// </summary>
    /// <param name="request">The request containing the selected user and the optional library filter.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ClearResumePointsRequest request, CancellationToken cancellationToken)
    {
        // The resume points belong to the selected user, so no intermediary may store any response of this route, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        string path = string.IsNullOrWhiteSpace(request.UserId)
            ? ApiRoutes.LibraryStatistics.CLEAR_RESUME_POINTS
            : ApiRoutes.LibraryStatistics.CLEAR_USER_RESUME_POINTS.Replace("{userId}", Uri.EscapeDataString(request.UserId), StringComparison.Ordinal);
        if (!string.IsNullOrWhiteSpace(request.LibraryId))
            path += $"?libraryId={Uri.EscapeDataString(request.LibraryId)}";
        await _apiHttpClient.DeleteAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
