#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetPlaybackManifest;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest</c> route.
/// </summary>
public class GetPlaybackManifestEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetPlaybackManifestQuery, Result<PlaybackManifestResponse>> _getPlaybackManifestQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpoint"/> class.
    /// </summary>
    /// <param name="getPlaybackManifestQueryHandler">Injected service for handling get playback manifest queries.</param>
    public GetPlaybackManifestEndpoint(IQueryHandler<GetPlaybackManifestQuery, Result<PlaybackManifestResponse>> getPlaybackManifestQueryHandler)
    {
        _getPlaybackManifestQueryHandler = getPlaybackManifestQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Tracks.GET_PLAYBACK_MANIFEST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the playback manifest of the track identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take unique identifiers from the route.
        string? libraryId = HttpContext.Request.RouteValues["libraryId"]?.ToString();
        string? artistId = HttpContext.Request.RouteValues["artistId"]?.ToString();
        string? albumId = HttpContext.Request.RouteValues["albumId"]?.ToString();
        string? trackId = HttpContext.Request.RouteValues["trackId"]?.ToString();
        // The capabilities the player advertises are optional; an absent parameter is an empty value, which the handler treats as no capabilities.
        string capabilities = HttpContext.Request.Query["capabilities"].ToString();
        // The stream path is filled in from the stream route itself, so that the manifest and the stream can never disagree on the shape of the route.
        // It is kept relative, without a host, a scheme or the version prefix, so that a client reaches the stream through its own application.
        string streamPath = ApiRoutes.Tracks.STREAM_TRACK
            .Replace("{libraryId}", libraryId, StringComparison.Ordinal)
            .Replace("{artistId}", artistId, StringComparison.Ordinal)
            .Replace("{albumId}", albumId, StringComparison.Ordinal)
            .Replace("{trackId}", trackId, StringComparison.Ordinal);
        // The segment route is filled in from the same route values, and its delivery handle and segment index placeholders are kept for the player
        // to expand, so the manifest and the segment route can never disagree on the shape of the path either. The delivery handle is what binds
        // the segments to the delivery this manifest declared; the capabilities are not repeated in the path.
        string segmentStreamPathTemplate = ApiRoutes.Tracks.STREAM_TRACK_SEGMENT
            .Replace("{libraryId}", libraryId, StringComparison.Ordinal)
            .Replace("{artistId}", artistId, StringComparison.Ordinal)
            .Replace("{albumId}", albumId, StringComparison.Ordinal)
            .Replace("{trackId}", trackId, StringComparison.Ordinal);
        GetPlaybackManifestQuery query = new(libraryId, artistId, albumId, trackId, capabilities, streamPath, segmentStreamPathTemplate);
        Result<PlaybackManifestResponse> result = await _getPlaybackManifestQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        return result.Match(success =>
        {
            // The manifest carries delivery metadata specific to the authenticated user, so no intermediary may store it and every response varies by
            // the bearer token that authorized it.
            HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
            HttpContext.Response.Headers["Vary"] = "Authorization";
            return TypedResults.Ok(success);
        }, Problem);
    }
}
