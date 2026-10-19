#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetPlayRecord;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.GetPlayRecord;

/// <summary>
/// API endpoint for the <c>/users/me/music-play-history/plays/{playRecordId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetPlayRecordEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetPlayRecordQuery, Result<PlayRecordResponse>> _getPlayRecordQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlayRecordEndpoint"/> class.
    /// </summary>
    /// <param name="getPlayRecordQueryHandler">Injected service for handling get play record queries.</param>
    public GetPlayRecordEndpoint(IQueryHandler<GetPlayRecordQuery, Result<PlayRecordResponse>> getPlayRecordQueryHandler)
    {
        _getPlayRecordQueryHandler = getPlayRecordQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.PlayHistory.GET_PLAY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets a recorded play of a track by the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The play record belongs to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        // Take the unique identifier from the route.
        string? playRecordId = HttpContext.Request.RouteValues["playRecordId"]?.ToString();
        Result<PlayRecordResponse> result = await _getPlayRecordQueryHandler.HandleAsync(new GetPlayRecordQuery(playRecordId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
