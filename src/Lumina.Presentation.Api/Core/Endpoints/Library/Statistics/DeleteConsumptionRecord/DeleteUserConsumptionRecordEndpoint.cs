#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.DeleteConsumptionRecord;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.DeleteConsumptionRecord;

/// <summary>
/// API endpoint for the <c>/users/{userId}/consumption-history/{recordId}</c> route.
/// </summary>
public class DeleteUserConsumptionRecordEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<DeleteConsumptionRecordCommand, Result<Deleted>> _deleteConsumptionRecordCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserConsumptionRecordEndpoint"/> class.
    /// </summary>
    /// <param name="deleteConsumptionRecordCommandHandler">Injected service for handling delete consumption record commands.</param>
    public DeleteUserConsumptionRecordEndpoint(ICommandHandler<DeleteConsumptionRecordCommand, Result<Deleted>> deleteConsumptionRecordCommandHandler)
    {
        _deleteConsumptionRecordCommandHandler = deleteConsumptionRecordCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.LibraryStatistics.DELETE_USER_CONSUMPTION_RECORD);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes one consumption record of a user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The consumption record belongs to the target user, and only that user or an Admin may read or delete it, so no intermediary may store any
        // response of this route, including its problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the unique identifiers from the route, so that they cannot be replaced by values supplied in the query string.
        string? targetUserId = HttpContext.Request.RouteValues["userId"]?.ToString();
        string? consumptionRecordId = HttpContext.Request.RouteValues["recordId"]?.ToString();
        // The access policy in the command decides whether the caller is the target user or an admin, and a regular user targeting another user
        // receives the same not authorized result as a route that names no accessible user.
        Result<Deleted> result = await _deleteConsumptionRecordCommandHandler.HandleAsync(new DeleteConsumptionRecordCommand(consumptionRecordId, targetUserId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
