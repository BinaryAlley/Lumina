#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.GetMusicPlayStatistics;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMusicPlayStatisticsEndpoint"/> API endpoint, for OpenAPI.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicPlayStatisticsEndpointSummary : Summary<GetMusicPlayStatisticsEndpoint>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicPlayStatisticsEndpointSummary"/> class.
    /// </summary>
    public GetMusicPlayStatisticsEndpointSummary()
    {
        Summary = "Gets the listening statistics of the current user.";
        Description = "Gets the total listening time, the number of plays that count as plays, the number of completed plays, the number of stopped plays that count and the completion rate of the current user. A play counts when it completed or when the listened duration reached 30 seconds, and the total listening time includes the listened duration of every recorded play. A play that is still in progress counts once it reaches the threshold, so the counted total can exceed the completed and stopped plays together. The statistics are aggregated over the plays of the authenticated user, so the response is private to that user.";

        ResponseParam<MusicPlayStatisticsResponse>(response => response.TotalListeningDurationInSeconds, "The total duration the user listened to, over every recorded play, in seconds.");
        ResponseParam<MusicPlayStatisticsResponse>(response => response.TotalPlayCount, "The number of recorded plays that count as plays. It includes an in-progress play that has already reached the threshold, so it need not equal the completed and stopped plays together.");
        ResponseParam<MusicPlayStatisticsResponse>(response => response.CompletedPlayCount, "The number of recorded plays that completed.");
        ResponseParam<MusicPlayStatisticsResponse>(response => response.StoppedPlayCount, "The number of recorded plays that stopped before the track completed and count as plays.");
        ResponseParam<MusicPlayStatisticsResponse>(response => response.CompletionRate, "The fraction of the counted plays that completed, between 0 and 1, or 0 when there is no counted play.");

        Response(200, "The listening statistics of the current user are returned.",
            example: new MusicPlayStatisticsResponse(
                TotalListeningDurationInSeconds: 93840.5,
                TotalPlayCount: 512,
                CompletedPlayCount: 388,
                StoppedPlayCount: 124,
                CompletionRate: 0.7578125)
        );

        Response(401, "Authentication required.", "application/problem+json",
            example: new[]
            {
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "Authentication failed",
                    instance = "/api/v1/users/me/music-play-history/statistics"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token has expired",
                    instance = "/api/v1/users/me/music-play-history/statistics"
                },
                new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    status = 401,
                    title = "Unauthorized",
                    detail = "The token is invalid",
                    instance = "/api/v1/users/me/music-play-history/statistics"
                }
            }
        );

        Response(403, "The request failed because the authenticated request does not carry the identity of a user.", "application/problem+json",
            example: new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                title = "General.Unauthorized",
                status = 403,
                detail = "NotAuthorized",
                instance = "/api/v1/users/me/music-play-history/statistics",
                traceId = "00-a712bbf99ca8ab485f86a762ae5ae74d-b3a2eb78813b0a5d-00"
            }
        );
    }
}
