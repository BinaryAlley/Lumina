#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary.PlayHistory;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.Statistics.GetMusicPlayStatistics;

/// <summary>
/// Class used for providing a textual description for the <see cref="GetMusicPlayStatisticsEndpoint"/> endpoint, for OpenAPI.
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
        Description = "Proxies the request that gets the total listening time, the counted plays, the completed plays, the stopped plays that count, and the completion rate of the current user to the API server. A play that is still in progress counts once it reaches the threshold, so the counted total can exceed the completed and stopped plays together.";

        ResponseParam<SuccessResponse<MusicPlayStatisticsDto>>(response => response.IsSuccess, "Whether the operation succeeded.");
        ResponseParam<MusicPlayStatisticsDto>(response => response.TotalListeningDurationInSeconds, "The total duration the user listened to, over every recorded play, in seconds.");
        ResponseParam<MusicPlayStatisticsDto>(response => response.TotalPlayCount, "The number of recorded plays that count as plays. It includes an in-progress play that has already reached the threshold, so it need not equal the completed and stopped plays together.");
        ResponseParam<MusicPlayStatisticsDto>(response => response.CompletedPlayCount, "The number of recorded plays that completed.");
        ResponseParam<MusicPlayStatisticsDto>(response => response.StoppedPlayCount, "The number of recorded plays that stopped before the track completed and count as plays.");
        ResponseParam<MusicPlayStatisticsDto>(response => response.CompletionRate, "The fraction of the counted plays that completed, between 0 and 1, or 0 when there is no counted play.");

        Response(200, "The listening statistics of the current user are returned.",
            example: new SuccessResponse<MusicPlayStatisticsDto>(true, new MusicPlayStatisticsDto(
                TotalListeningDurationInSeconds: 93840.5,
                TotalPlayCount: 512,
                CompletedPlayCount: 388,
                StoppedPlayCount: 124,
                CompletionRate: 0.7578125))
        );
    }
}
