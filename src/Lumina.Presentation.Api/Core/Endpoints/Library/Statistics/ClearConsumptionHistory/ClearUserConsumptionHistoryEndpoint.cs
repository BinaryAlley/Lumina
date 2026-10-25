#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.ClearConsumptionHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.ClearConsumptionHistory;

/// <summary>
/// API endpoint for the <c>/users/{userId}/consumption-history</c> route.
/// </summary>
public class ClearUserConsumptionHistoryEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<ClearConsumptionHistoryCommand, Result<Deleted>> _clearConsumptionHistoryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearUserConsumptionHistoryEndpoint"/> class.
    /// </summary>
    /// <param name="clearConsumptionHistoryCommandHandler">Injected service for handling clear consumption history commands.</param>
    public ClearUserConsumptionHistoryEndpoint(ICommandHandler<ClearConsumptionHistoryCommand, Result<Deleted>> clearConsumptionHistoryCommandHandler)
    {
        _clearConsumptionHistoryCommandHandler = clearConsumptionHistoryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.LibraryStatistics.CLEAR_USER_CONSUMPTION_HISTORY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes every consumption record of a user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The consumption history belongs to the target user, and only that user or an Admin may read or delete it, so no intermediary may store any
        // response of this route, including its problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the target user from the route. The access policy in the command decides whether the caller is the target user or an admin, and a
        // regular user targeting another user receives the same not authorized result as a route that names no accessible user.
        string? targetUserId = HttpContext.Request.RouteValues["userId"]?.ToString();
        Result<Deleted> result = await _clearConsumptionHistoryCommandHandler.HandleAsync(new ClearConsumptionHistoryCommand(targetUserId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
