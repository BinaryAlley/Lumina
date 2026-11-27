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
/// API endpoint for the <c>/users/me/consumption-history/{recordId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class DeleteConsumptionRecordEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<DeleteConsumptionRecordCommand, Result<Deleted>> _deleteConsumptionRecordCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConsumptionRecordEndpoint"/> class.
    /// </summary>
    /// <param name="deleteConsumptionRecordCommandHandler">Injected service for handling delete consumption record commands.</param>
    public DeleteConsumptionRecordEndpoint(ICommandHandler<DeleteConsumptionRecordCommand, Result<Deleted>> deleteConsumptionRecordCommandHandler)
    {
        _deleteConsumptionRecordCommandHandler = deleteConsumptionRecordCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.LibraryStatistics.DELETE_CONSUMPTION_RECORD);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes one consumption record of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The consumption record belongs to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the unique identifier from the route, so that it cannot be replaced by a value supplied in the query string.
        string? consumptionRecordId = HttpContext.Request.RouteValues["recordId"]?.ToString();
        Result<Deleted> result = await _deleteConsumptionRecordCommandHandler.HandleAsync(new DeleteConsumptionRecordCommand(consumptionRecordId, TargetUserId: null), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
