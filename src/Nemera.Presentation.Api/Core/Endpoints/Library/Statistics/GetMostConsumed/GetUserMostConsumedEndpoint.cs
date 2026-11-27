#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaConsumption;
using Lumina.Application.Core.MediaConsumption.Queries.GetMostConsumed;
using Lumina.Contracts.Requests.MediaConsumption;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.Statistics;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.Statistics.GetMostConsumed;

/// <summary>
/// API endpoint for the <c>/users/{userId}/consumption-history/most-consumed</c> route.
/// </summary>
public class GetUserMostConsumedEndpoint : BaseEndpoint<GetMostConsumedRequest, IResult>
{
    private readonly IQueryHandler<GetMostConsumedQuery, Result<PaginatedResponse<ConsumedItemResponse>>> _getMostConsumedQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserMostConsumedEndpoint"/> class.
    /// </summary>
    /// <param name="getMostConsumedQueryHandler">Injected service for handling get most consumed queries.</param>
    public GetUserMostConsumedEndpoint(IQueryHandler<GetMostConsumedQuery, Result<PaginatedResponse<ConsumedItemResponse>>> getMostConsumedQueryHandler)
    {
        _getMostConsumedQueryHandler = getMostConsumedQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.LibraryStatistics.GET_USER_MOST_CONSUMED);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the most consumed media items of a user.
    /// </summary>
    /// <param name="request">The request containing the optional filters and the page bounds.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMostConsumedRequest request, CancellationToken cancellationToken)
    {
        // The consumption history belongs to the target user, and only that user or an Admin may read it, so no intermediary may store any response of
        // this route, including its problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the target user from the route. The access policy in the query decides whether the caller is the target user or an admin, and a
        // regular user targeting another user receives the same not authorized result as a route that names no accessible user.
        string? targetUserId = HttpContext.Request.RouteValues["userId"]?.ToString();
        Result<PaginatedResponse<ConsumedItemResponse>> result = await _getMostConsumedQueryHandler.HandleAsync(request.ToQuery(targetUserId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
