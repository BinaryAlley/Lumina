#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.DeleteTrack;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteTrack;

/// <summary>
/// API endpoint for the <c>/artists/{artistId}/albums/{albumId}/tracks/{trackId}</c> route.
/// </summary>
public class DeleteTrackEndpoint : BaseEndpoint<DeleteTrackRequest, IResult>
{
    private readonly ICommandHandler<DeleteTrackCommand, Result<Deleted>> _deleteTrackCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTrackEndpoint"/> class.
    /// </summary>
    /// <param name="deleteTrackCommandHandler">Injected service for handling delete track commands.</param>
    public DeleteTrackEndpoint(ICommandHandler<DeleteTrackCommand, Result<Deleted>> deleteTrackCommandHandler)
    {
        _deleteTrackCommandHandler = deleteTrackCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Tracks.DELETE_TRACK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes a track by Id.
    /// </summary>
    /// <param name="request">The request containing the id of the track to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(DeleteTrackRequest request, CancellationToken cancellationToken)
    {
        Result<Deleted> result = await _deleteTrackCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
