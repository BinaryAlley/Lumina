#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Commands.AddTrack;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.AddTrack;

/// <summary>
/// API endpoint for the <c>/{libraryId}/artists/{artistId}/albums/{albumId}/tracks</c> route.
/// </summary>
public class AddTrackEndpoint : BaseEndpoint<AddTrackRequest, IResult>
{
    private readonly ICommandHandler<AddTrackCommand, Result<TrackResponse>> _addTrackCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTrackEndpoint"/> class.
    /// </summary>
    /// <param name="addTrackCommandHandler">Injected service for handling add track commands.</param>
    public AddTrackEndpoint(ICommandHandler<AddTrackCommand, Result<TrackResponse>> addTrackCommandHandler)
    {
        _addTrackCommandHandler = addTrackCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Albums.ADD_TRACK);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds a track stored in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the track to be added.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddTrackRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        Result<TrackResponse> result = await _addTrackCommandHandler.HandleAsync(request.ToCommand(libraryId, artistId, albumId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Created($"{BaseURL}api/v1/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{result.Value.Id}", result.Value), Problem);
    }
}
