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
/// API endpoint for the <c>/users/me/media-resume-points</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class ClearResumePointsEndpoint : BaseEndpoint<ClearResumePointsRequest, IResult>
{
    private readonly ICommandHandler<ClearResumePointsCommand, Result<Deleted>> _clearResumePointsCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearResumePointsEndpoint"/> class.
    /// </summary>
    /// <param name="clearResumePointsCommandHandler">Injected service for handling clear resume points commands.</param>
    public ClearResumePointsEndpoint(ICommandHandler<ClearResumePointsCommand, Result<Deleted>> clearResumePointsCommandHandler)
    {
        _clearResumePointsCommandHandler = clearResumePointsCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.LibraryStatistics.CLEAR_RESUME_POINTS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes the resume points of the current user.
    /// </summary>
    /// <param name="request">The request containing the optional library filter.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ClearResumePointsRequest request, CancellationToken cancellationToken)
    {
        // The resume points belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        Result<Deleted> result = await _clearResumePointsCommandHandler.HandleAsync(request.ToCommand(targetUserId: null), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
