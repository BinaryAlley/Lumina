#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Commands.StartConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.MediaConsumption;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.StartConsumption;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/media-items/{mediaKind}/{mediaItemId}/consumption-records</c> route.
/// </summary>
public class StartConsumptionEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<StartConsumptionCommand, Result<ConsumptionRecordResponse>> _startConsumptionCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartConsumptionEndpoint"/> class.
    /// </summary>
    /// <param name="startConsumptionCommandHandler">Injected service for handling start consumption commands.</param>
    public StartConsumptionEndpoint(ICommandHandler<StartConsumptionCommand, Result<ConsumptionRecordResponse>> startConsumptionCommandHandler)
    {
        _startConsumptionCommandHandler = startConsumptionCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.MediaConsumption.START_CONSUMPTION);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Starts a media consumption session of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The consumption record belongs to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the unique identifiers from the route, so that they cannot be replaced by values posted in a body.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? mediaKind = HttpContext.Request.RouteValues["mediaKind"]?.ToString();
        string? mediaItemId = HttpContext.Request.RouteValues["mediaItemId"]?.ToString();
        Result<ConsumptionRecordResponse> result = await _startConsumptionCommandHandler.HandleAsync(new StartConsumptionCommand(libraryId, mediaKind, mediaItemId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
