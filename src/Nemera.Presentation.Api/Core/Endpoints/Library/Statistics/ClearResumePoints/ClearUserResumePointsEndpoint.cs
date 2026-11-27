#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Commands.ClearResumePoints;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.ClearResumePoints;

/// <summary>
/// API endpoint for the <c>/users/{userId}/media-resume-points</c> route.
/// </summary>
public class ClearUserResumePointsEndpoint : BaseEndpoint<ClearResumePointsRequest, IResult>
{
    private readonly ICommandHandler<ClearResumePointsCommand, Result<Deleted>> _clearResumePointsCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearUserResumePointsEndpoint"/> class.
    /// </summary>
    /// <param name="clearResumePointsCommandHandler">Injected service for handling clear resume points commands.</param>
    public ClearUserResumePointsEndpoint(ICommandHandler<ClearResumePointsCommand, Result<Deleted>> clearResumePointsCommandHandler)
    {
        _clearResumePointsCommandHandler = clearResumePointsCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.LibraryStatistics.CLEAR_USER_RESUME_POINTS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes the resume points of a user.
    /// </summary>
    /// <param name="request">The request containing the optional library filter.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ClearResumePointsRequest request, CancellationToken cancellationToken)
    {
        // The resume points belong to the target user, and only that user or an Admin may read or delete them, so no intermediary may store any
        // response of this route, including its problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the target user from the route. The access policy in the command decides whether the caller is the target user or an admin, and a
        // regular user targeting another user receives the same not authorized result as a route that names no accessible user.
        string? targetUserId = HttpContext.Request.RouteValues["userId"]?.ToString();
        Result<Deleted> result = await _clearResumePointsCommandHandler.HandleAsync(request.ToCommand(targetUserId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
