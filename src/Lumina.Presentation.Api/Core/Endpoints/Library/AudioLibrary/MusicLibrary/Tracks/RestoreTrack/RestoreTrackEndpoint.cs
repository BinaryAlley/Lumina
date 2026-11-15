#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.RestoreTrack;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.RestoreTrack;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore</c> route.
/// </summary>
public class RestoreTrackEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<RestoreTrackCommand, Result<Updated>> _restoreTrackCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreTrackEndpoint"/> class.
    /// </summary>
    /// <param name="restoreTrackCommandHandler">Injected service for handling restore track commands.</param>
    public RestoreTrackEndpoint(ICommandHandler<RestoreTrackCommand, Result<Updated>> restoreTrackCommandHandler)
    {
        _restoreTrackCommandHandler = restoreTrackCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Tracks.RESTORE_TRACK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Restores the deleted track identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        string? trackId = HttpContext.Request.RouteValues["trackId"]?.ToString();
        Result<Updated> result = await _restoreTrackCommandHandler.HandleAsync(new RestoreTrackCommand(libraryId, artistId, albumId, trackId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
