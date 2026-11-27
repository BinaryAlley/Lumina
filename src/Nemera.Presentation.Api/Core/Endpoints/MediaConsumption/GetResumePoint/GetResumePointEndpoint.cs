#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaConsumption.Queries.GetResumePoint;
using Lumina.Contracts.Responses.MediaConsumption;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.MediaConsumption;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.MediaConsumption.GetResumePoint;

/// <summary>
/// API endpoint for the <c>/users/me/media-resume-points/{mediaKind}/{mediaItemId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetResumePointEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetResumePointQuery, Result<ResumePointResponse>> _getResumePointQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetResumePointEndpoint"/> class.
    /// </summary>
    /// <param name="getResumePointQueryHandler">Injected service for handling get resume point queries.</param>
    public GetResumePointEndpoint(IQueryHandler<GetResumePointQuery, Result<ResumePointResponse>> getResumePointQueryHandler)
    {
        _getResumePointQueryHandler = getResumePointQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.MediaConsumption.GET_RESUME_POINT);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the resume point of the current user for a media item.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The resume point belongs to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers.Append("Vary", "Authorization");
        // Take the unique identifiers from the route.
        string? mediaKind = HttpContext.Request.RouteValues["mediaKind"]?.ToString();
        string? mediaItemId = HttpContext.Request.RouteValues["mediaItemId"]?.ToString();
        Result<ResumePointResponse> result = await _getResumePointQueryHandler.HandleAsync(new GetResumePointQuery(mediaKind, mediaItemId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
