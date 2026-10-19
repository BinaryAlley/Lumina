#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.ReportPlayProgress;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.ReportPlayProgress;

/// <summary>
/// API endpoint for the <c>/users/me/music-play-history/plays/{playRecordId}/progress</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class ReportPlayProgressEndpoint : BaseEndpoint<ReportPlayProgressRequest, IResult>
{
    private readonly ICommandHandler<ReportPlayProgressCommand, Result<Updated>> _reportPlayProgressCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportPlayProgressEndpoint"/> class.
    /// </summary>
    /// <param name="reportPlayProgressCommandHandler">Injected service for handling report play progress commands.</param>
    public ReportPlayProgressEndpoint(ICommandHandler<ReportPlayProgressCommand, Result<Updated>> reportPlayProgressCommandHandler)
    {
        _reportPlayProgressCommandHandler = reportPlayProgressCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.PlayHistory.REPORT_PLAY_PROGRESS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Reports the progress of a play of a track by the current user.
    /// </summary>
    /// <param name="request">The request containing the last position and the listened duration.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(ReportPlayProgressRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier from the route.
        string? playRecordId = HttpContext.Request.RouteValues["playRecordId"]?.ToString();
        Result<Updated> result = await _reportPlayProgressCommandHandler.HandleAsync(request.ToCommand(playRecordId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
