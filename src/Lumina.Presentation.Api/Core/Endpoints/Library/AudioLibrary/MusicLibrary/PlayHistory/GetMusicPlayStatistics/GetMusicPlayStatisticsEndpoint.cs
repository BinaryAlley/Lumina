#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetMusicPlayStatistics;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.GetMusicPlayStatistics;

/// <summary>
/// API endpoint for the <c>/users/me/music-play-history/statistics</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetMusicPlayStatisticsEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetMusicPlayStatisticsQuery, Result<MusicPlayStatisticsResponse>> _getMusicPlayStatisticsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicPlayStatisticsEndpoint"/> class.
    /// </summary>
    /// <param name="getMusicPlayStatisticsQueryHandler">Injected service for handling get music play statistics queries.</param>
    public GetMusicPlayStatisticsEndpoint(IQueryHandler<GetMusicPlayStatisticsQuery, Result<MusicPlayStatisticsResponse>> getMusicPlayStatisticsQueryHandler)
    {
        _getMusicPlayStatisticsQueryHandler = getMusicPlayStatisticsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.PlayStatistics.GET_MUSIC_PLAY_STATISTICS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the listening statistics of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The statistics belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        Result<MusicPlayStatisticsResponse> result = await _getMusicPlayStatisticsQueryHandler.HandleAsync(new GetMusicPlayStatisticsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
