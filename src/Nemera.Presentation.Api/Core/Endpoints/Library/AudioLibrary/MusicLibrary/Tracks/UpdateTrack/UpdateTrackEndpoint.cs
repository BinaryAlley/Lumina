#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.UpdateTrack;
using Nemera.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.UpdateTrack;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}</c> route.
/// </summary>
public class UpdateTrackEndpoint : BaseEndpoint<UpdateTrackRequest, IResult>
{
    private readonly ICommandHandler<UpdateTrackCommand, Result<TrackResponse>> _updateTrackCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTrackEndpoint"/> class.
    /// </summary>
    /// <param name="updateTrackCommandHandler">Injected service for handling update track commands.</param>
    public UpdateTrackEndpoint(ICommandHandler<UpdateTrackCommand, Result<TrackResponse>> updateTrackCommandHandler)
    {
        _updateTrackCommandHandler = updateTrackCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Tracks.UPDATE_TRACK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates a track stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the track to be updated.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateTrackRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        string? trackId = HttpContext.Request.RouteValues["trackId"]?.ToString();
        Result<TrackResponse> result = await _updateTrackCommandHandler.HandleAsync(request.ToCommand(libraryId, artistId, albumId, trackId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
