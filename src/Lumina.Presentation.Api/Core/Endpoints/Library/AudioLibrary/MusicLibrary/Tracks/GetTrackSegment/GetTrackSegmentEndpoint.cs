#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackSegment;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.DependencyInjection;
using Lumina.Presentation.Api.Common.Http;
using Lumina.Presentation.Api.Common.MimeTypes;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackSegment;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream/deliveries/{deliveryId}/segments/{segmentIndex}</c> route.
/// </summary>
public class GetTrackSegmentEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    // The name is repeated in the web client application, which is a separate project that keeps its own contracts, so both sides must change together.
    private const string SEGMENT_COUNT_HEADER_NAME = "X-Lumina-Segment-Count";
    private const string SEGMENT_START_SECONDS_HEADER_NAME = "X-Segment-Start-Seconds";
    private const string TRACK_DURATION_SECONDS_HEADER_NAME = "X-Lumina-Track-Duration-Seconds";

    private readonly IQueryHandler<GetTrackSegmentQuery, Result<NormalizedTrackSegmentDataDto>> _getTrackSegmentQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackSegmentEndpoint"/> class.
    /// </summary>
    /// <param name="getTrackSegmentQueryHandler">Injected service for handling get track segment queries.</param>
    public GetTrackSegmentEndpoint(IQueryHandler<GetTrackSegmentQuery, Result<NormalizedTrackSegmentDataDto>> getTrackSegmentQueryHandler)
    {
        _getTrackSegmentQueryHandler = getTrackSegmentQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Tracks.STREAM_TRACK_SEGMENT);
        Version(1);
        DontCatchExceptions();
        // The segment stream is limited per user with the same concurrency limiter as the track stream, and it is excluded from the eventual
        // consistency pipeline, because a long lived response would otherwise hold the request's database transaction open for its whole duration.
        Options(builder => builder
            .RequireRateLimiting(PresentationApiLayerServices.TRACK_STREAM_RATE_LIMIT_POLICY)
            .WithMetadata(new SkipEventualConsistencyMetadata()));
    }

    /// <summary>
    /// Streams one segment of the normalized audio of the track identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers and the segment index from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        string? trackId = HttpContext.Request.RouteValues["trackId"]?.ToString();
        string? deliveryId = HttpContext.Request.RouteValues["deliveryId"]?.ToString();
        string? segmentIndex = HttpContext.Request.RouteValues["segmentIndex"]?.ToString();
        GetTrackSegmentQuery query = new(libraryId, artistId, albumId, trackId, deliveryId, segmentIndex);
        Result<NormalizedTrackSegmentDataDto> result = await _getTrackSegmentQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return Problem(result.Errors);

        // The produced segment is private to the authenticated user and is served from a temporary cache, so it must never be stored by an
        // intermediary, and content sniffing stays disabled, so a segment can never be interpreted as active content.
        HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        // The count lets the player stop at the end of the production instead of probing for it with a request that cannot be told apart
        // from a track that was deleted or moved. It is sent on every segment, because any segment can be the first one a player fetches.
        HttpContext.Response.Headers[SEGMENT_COUNT_HEADER_NAME] = result.Value.SegmentCount.ToString();
        // The real start of the segment, because ffmpeg cuts at the first packet at or after a segment boundary and a container can number its
        // frames from the start of the track. The player offsets the packets by this value instead of assuming an exact multiple of the duration.
        HttpContext.Response.Headers[SEGMENT_START_SECONDS_HEADER_NAME] = result.Value.StartSeconds.ToString(CultureInfo.InvariantCulture);
        // The produced duration can differ from the probed duration, so the player uses this value for its timeline and progress bar.
        HttpContext.Response.Headers[TRACK_DURATION_SECONDS_HEADER_NAME] = result.Value.DurationInSeconds.ToString(CultureInfo.InvariantCulture);
        // The length comes from the index of the production, so the response is length delimited without reading the segment into memory first.
        return new DisposingStreamResult(result.Value.Content, MusicMimeTypes.GetContainerMimeType(result.Value.Container), result.Value.Length);
    }
}
