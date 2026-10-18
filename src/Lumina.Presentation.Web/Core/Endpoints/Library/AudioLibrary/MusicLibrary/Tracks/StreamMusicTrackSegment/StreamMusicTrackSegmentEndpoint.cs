#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DependencyInjection;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Http;
using Lumina.Presentation.Web.Common.Playback;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrackSegment;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track-segment/{deliveryId}/{segmentIndex}</c> route.
/// </summary>
public class StreamMusicTrackSegmentEndpoint : BaseEndpoint<StreamMusicTrackSegmentRequest, IResult>
{
    private readonly IStreamingApiHttpClient _streamingApiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackSegmentEndpoint"/> class.
    /// </summary>
    /// <param name="streamingApiHttpClient">Injected service for streaming interactions with the API.</param>
    public StreamMusicTrackSegmentEndpoint(IStreamingApiHttpClient streamingApiHttpClient)
    {
        _streamingApiHttpClient = streamingApiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Music.STREAM_TRACK_SEGMENT);
        DontAutoTag();
        // The segment stream is bounded per user with the same concurrency limit as the track stream, so a single user can never hold an
        // unbounded number of long lived responses open at once and exhaust the connections towards the API.
        Options(options => options
            .WithTags("Music")
            .RequireRateLimiting(PresentationWebLayerServices.TRACK_STREAM_RATE_LIMIT_POLICY));
    }

    /// <summary>
    /// Streams one segment of the normalized audio of the track identified by <paramref name="request"/> through the API, without buffering it.
    /// </summary>
    /// <param name="request">The request containing the Ids of the artist, album, track and media library, the delivery handle, and the segment index.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(StreamMusicTrackSegmentRequest request, CancellationToken cancellationToken)
    {
        // The route values are normalized to Guids before they are substituted into the upstream URL, so that a crafted route value can never
        // escape its URL segment, and an unparseable id is reported by the API as a missing id.
        string normalizedArtistId = Guid.TryParse(request.ArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string normalizedAlbumId = Guid.TryParse(request.AlbumId, out Guid parsedAlbumId) ? parsedAlbumId.ToString() : Guid.Empty.ToString();
        string normalizedTrackId = Guid.TryParse(request.TrackId, out Guid parsedTrackId) ? parsedTrackId.ToString() : Guid.Empty.ToString();
        // The handle has exactly one accepted format, so it is never normalized into another value that would silently address a different delivery.
        if (!PlaybackDeliveryId.IsValid(request.DeliveryId))
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            HttpContext.Response.ContentLength = 0;
            return Results.Empty;
        }
        string endpoint = ApiRoutes.Music.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", normalizedArtistId)
            .Replace("{albumId}", normalizedAlbumId)
            .Replace("{trackId}", normalizedTrackId)
            .Replace("{deliveryId}", request.DeliveryId)
            .Replace("{segmentIndex}", request.SegmentIndex.ToString());

        // The API response is disposed once the copy ends, so that the underlying connection is released as soon as the client stops reading.
        ApiStreamResult streamResult;
        try
        {
            streamResult = await _streamingApiHttpClient.GetStreamAsync(endpoint, new Dictionary<string, string>(), cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException apiException) when (apiException.HttpStatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.TooManyRequests)
        {
            // A busy normalization pipeline or the stream concurrency limit is an expected, transient condition that the player retries. It is
            // answered with a bare status instead of being logged as an unexpected API failure with a stack trace.
            HttpContext.Response.StatusCode = (int)apiException.HttpStatusCode;
            HttpContext.Response.ContentLength = 0;
            return Results.Empty;
        }
        await using (streamResult) // The proxy owns the status, header, caching and body rules shared with the direct track stream proxy.
            await ApiStreamProxy.WriteAsync(HttpContext.Response, streamResult, cancellationToken).ConfigureAwait(false);
        return Results.Empty;
    }
}
