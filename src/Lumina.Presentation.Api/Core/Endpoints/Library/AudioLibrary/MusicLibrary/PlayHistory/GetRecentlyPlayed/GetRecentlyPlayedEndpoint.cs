#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Queries.GetRecentlyPlayed;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Contracts.Responses.Common;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.GetRecentlyPlayed;

/// <summary>
/// API endpoint for the <c>/users/me/music-play-history/recently-played</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetRecentlyPlayedEndpoint : BaseEndpoint<GetRecentlyPlayedRequest, IResult>
{
    private readonly IQueryHandler<GetRecentlyPlayedQuery, Result<PaginatedResponse<PlayedTrackResponse>>> _getRecentlyPlayedQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetRecentlyPlayedEndpoint"/> class.
    /// </summary>
    /// <param name="getRecentlyPlayedQueryHandler">Injected service for handling get recently played queries.</param>
    public GetRecentlyPlayedEndpoint(IQueryHandler<GetRecentlyPlayedQuery, Result<PaginatedResponse<PlayedTrackResponse>>> getRecentlyPlayedQueryHandler)
    {
        _getRecentlyPlayedQueryHandler = getRecentlyPlayedQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.PlayStatistics.GET_RECENTLY_PLAYED);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the recently played tracks of the current user.
    /// </summary>
    /// <param name="request">The request containing the optional page bounds.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetRecentlyPlayedRequest request, CancellationToken cancellationToken)
    {
        // The statistics belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        Result<PaginatedResponse<PlayedTrackResponse>> result = await _getRecentlyPlayedQueryHandler.HandleAsync(request.ToQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
