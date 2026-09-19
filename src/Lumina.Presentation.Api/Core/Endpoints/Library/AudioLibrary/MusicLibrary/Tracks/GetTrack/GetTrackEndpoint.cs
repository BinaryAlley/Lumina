#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrack;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrack;

/// <summary>
/// API endpoint for the <c>/artists/{artistId}/albums/{albumId}/tracks/{trackId}</c> route.
/// </summary>
public class GetTrackEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetTrackQuery, Result<TrackResponse>> _getTrackQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackEndpoint"/> class.
    /// </summary>
    /// <param name="getTrackQueryHandler">Injected service for handling get track queries.</param>
    public GetTrackEndpoint(IQueryHandler<GetTrackQuery, Result<TrackResponse>> getTrackQueryHandler)
    {
        _getTrackQueryHandler = getTrackQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Tracks.GET_TRACK_BY_ID);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets a track by Id.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        string? trackId = HttpContext.Request.RouteValues["trackId"]?.ToString();
        GetTrackQuery query = new(libraryId, artistId, albumId, trackId);
        Result<TrackResponse> result = await _getTrackQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
