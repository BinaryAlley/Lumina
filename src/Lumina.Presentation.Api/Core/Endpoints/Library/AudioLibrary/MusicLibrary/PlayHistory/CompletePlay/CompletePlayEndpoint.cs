#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.CompletePlay;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.CompletePlay;

/// <summary>
/// API endpoint for the <c>/users/me/music-play-history/plays/{playRecordId}/complete</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class CompletePlayEndpoint : BaseEndpoint<CompletePlayRequest, IResult>
{
    private readonly ICommandHandler<CompletePlayCommand, Result<Updated>> _completePlayCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompletePlayEndpoint"/> class.
    /// </summary>
    /// <param name="completePlayCommandHandler">Injected service for handling complete play commands.</param>
    public CompletePlayEndpoint(ICommandHandler<CompletePlayCommand, Result<Updated>> completePlayCommandHandler)
    {
        _completePlayCommandHandler = completePlayCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.PlayHistory.COMPLETE_PLAY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Reports that a play of a track by the current user reached its end.
    /// </summary>
    /// <param name="request">The request containing the last position and the listened duration.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CompletePlayRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier from the route.
        string? playRecordId = HttpContext.Request.RouteValues["playRecordId"]?.ToString();
        Result<Updated> result = await _completePlayCommandHandler.HandleAsync(request.ToCommand(playRecordId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
