#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory.Commands.StartPlay;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.PlayHistory.StartPlay;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/plays</c> route.
/// </summary>
public class StartPlayEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<StartPlayCommand, Result<PlayRecordResponse>> _startPlayCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartPlayEndpoint"/> class.
    /// </summary>
    /// <param name="startPlayCommandHandler">Injected service for handling start play commands.</param>
    public StartPlayEndpoint(ICommandHandler<StartPlayCommand, Result<PlayRecordResponse>> startPlayCommandHandler)
    {
        _startPlayCommandHandler = startPlayCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.PlayHistory.START_PLAY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Records the start of a play of a track by the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifiers from the route, so that they cannot be replaced by values posted in a body.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        string? trackId = HttpContext.Request.RouteValues["trackId"]?.ToString();
        Result<PlayRecordResponse> result = await _startPlayCommandHandler
            .HandleAsync(new StartPlayCommand(libraryId, artistId, albumId, trackId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
