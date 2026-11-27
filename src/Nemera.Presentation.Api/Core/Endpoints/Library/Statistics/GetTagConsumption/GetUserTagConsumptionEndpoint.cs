#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetTagConsumption;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetTagConsumption;

/// <summary>
/// API endpoint for the <c>/users/{userId}/consumption-statistics/tags</c> route.
/// </summary>
public class GetUserTagConsumptionEndpoint : BaseEndpoint<GetTagConsumptionRequest, IResult>
{
    private readonly IQueryHandler<GetTagConsumptionQuery, Result<IReadOnlyList<TagConsumptionResponse>>> _getTagConsumptionQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserTagConsumptionEndpoint"/> class.
    /// </summary>
    /// <param name="getTagConsumptionQueryHandler">Injected service for handling get tag consumption queries.</param>
    public GetUserTagConsumptionEndpoint(IQueryHandler<GetTagConsumptionQuery, Result<IReadOnlyList<TagConsumptionResponse>>> getTagConsumptionQueryHandler)
    {
        _getTagConsumptionQueryHandler = getTagConsumptionQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.LibraryStatistics.GET_USER_TAG_CONSUMPTION);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the consumption statistics of the tags of a user.
    /// </summary>
    /// <param name="request">The request containing the optional filters and the date range.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetTagConsumptionRequest request, CancellationToken cancellationToken)
    {
        // The consumption statistics belong to the target user, and only that user or an Admin may read them, so no intermediary may store any response
        // of this route, including its problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the target user from the route. The access policy in the query decides whether the caller is the target user or an admin, and a
        // regular user targeting another user receives the same not authorized result as a route that names no accessible user.
        string? targetUserId = HttpContext.Request.RouteValues["userId"]?.ToString();
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _getTagConsumptionQueryHandler.HandleAsync(request.ToQuery(targetUserId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
