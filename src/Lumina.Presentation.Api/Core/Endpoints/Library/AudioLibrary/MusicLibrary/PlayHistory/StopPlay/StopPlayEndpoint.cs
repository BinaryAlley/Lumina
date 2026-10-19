#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.StopPlay;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.StopPlay;

/// <summary>
/// API endpoint for the <c>/users/me/music-play-history/plays/{playRecordId}/stop</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class StopPlayEndpoint : BaseEndpoint<StopPlayRequest, IResult>
{
    private readonly ICommandHandler<StopPlayCommand, Result<Updated>> _stopPlayCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="StopPlayEndpoint"/> class.
    /// </summary>
    /// <param name="stopPlayCommandHandler">Injected service for handling stop play commands.</param>
    public StopPlayEndpoint(ICommandHandler<StopPlayCommand, Result<Updated>> stopPlayCommandHandler)
    {
        _stopPlayCommandHandler = stopPlayCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.PlayHistory.STOP_PLAY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Reports that a play of a track by the current user stopped before the track completed.
    /// </summary>
    /// <param name="request">The request containing the last position and the listened duration.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(StopPlayRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier from the route.
        string? playRecordId = HttpContext.Request.RouteValues["playRecordId"]?.ToString();
        Result<Updated> result = await _stopPlayCommandHandler.HandleAsync(request.ToCommand(playRecordId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
