#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.MediaConsumption;
using Nemera.Application.Core.MediaConsumption.Queries.GetTagConsumption;
using Nemera.Contracts.Requests.MediaConsumption;
using Nemera.Contracts.Responses.MediaConsumption;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.Statistics;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.Statistics.GetTagConsumption;

/// <summary>
/// API endpoint for the <c>/users/me/consumption-statistics/tags</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetTagConsumptionEndpoint : BaseEndpoint<GetTagConsumptionRequest, IResult>
{
    private readonly IQueryHandler<GetTagConsumptionQuery, Result<IReadOnlyList<TagConsumptionResponse>>> _getTagConsumptionQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTagConsumptionEndpoint"/> class.
    /// </summary>
    /// <param name="getTagConsumptionQueryHandler">Injected service for handling get tag consumption queries.</param>
    public GetTagConsumptionEndpoint(IQueryHandler<GetTagConsumptionQuery, Result<IReadOnlyList<TagConsumptionResponse>>> getTagConsumptionQueryHandler)
    {
        _getTagConsumptionQueryHandler = getTagConsumptionQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.LibraryStatistics.GET_TAG_CONSUMPTION);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the consumption statistics of the tags of the current user.
    /// </summary>
    /// <param name="request">The request containing the optional filters and the date range.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetTagConsumptionRequest request, CancellationToken cancellationToken)
    {
        // The consumption statistics belong to the authenticated user, so no intermediary may store any response of this route, including its
        // problem responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        Result<IReadOnlyList<TagConsumptionResponse>> result = await _getTagConsumptionQueryHandler.HandleAsync(request.ToQuery(targetUserId: null), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
