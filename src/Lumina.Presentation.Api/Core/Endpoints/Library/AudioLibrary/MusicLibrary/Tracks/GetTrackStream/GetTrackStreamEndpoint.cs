#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetTrackStream;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.DependencyInjection;
using Lumina.Presentation.Api.Common.Http;
using Lumina.Presentation.Api.Common.MimeTypes;
using Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetTrackStream;

/// <summary>
/// API endpoint for the <c>/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream</c> route.
/// </summary>
public class GetTrackStreamEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetTrackStreamQuery, Result<TrackStreamDataDto>> _getTrackStreamQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTrackStreamEndpoint"/> class.
    /// </summary>
    /// <param name="getTrackStreamQueryHandler">Injected service for handling get track stream queries.</param>
    public GetTrackStreamEndpoint(IQueryHandler<GetTrackStreamQuery, Result<TrackStreamDataDto>> getTrackStreamQueryHandler)
    {
        _getTrackStreamQueryHandler = getTrackStreamQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Tracks.STREAM_TRACK);
        Version(1);
        DontCatchExceptions();
        // The stream is limited per user with a concurrency limiter (never with the login rate limit policy), and it is excluded from the eventual
        // consistency pipeline, because a long lived audio response would otherwise hold the request's database transaction open for its whole duration.
        Options(builder => builder
            .RequireRateLimiting(PresentationApiLayerServices.TRACK_STREAM_RATE_LIMIT_POLICY)
            .WithMetadata(new SkipEventualConsistencyMetadata()));
    }

    /// <summary>
    /// Streams the audio file of the track identified by the route.
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
        GetTrackStreamQuery query = new(libraryId, artistId, albumId, trackId);
        Result<TrackStreamDataDto> result = await _getTrackStreamQueryHandler.HandleAsync(query, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
            return Problem(result.Errors);

        // Audio is inert content, but it is still served with content sniffing disabled, so that a file with an unexpected extension is never interpreted as active content.
        HttpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";
        // The audio is private to the authenticated user and its bytes must never be stored by an intermediary, so it is not publicly cacheable and it varies by the bearer token.
        HttpContext.Response.Headers["Cache-Control"] = "private";
        HttpContext.Response.Headers["Vary"] = "Authorization";

        // The entity tag is derived from the size and the last modification time of the file, never from its path or its name, so no file system detail is exposed, and no download name is passed, so no Content-Disposition is emitted.
        EntityTagHeaderValue entityTag = new($"\"{result.Value.LastModified.ToUnixTimeSeconds()}-{result.Value.Length}\"");
        return TypedResults.PhysicalFile(
            result.Value.Path,
            MusicMimeTypes.GetMimeType(result.Value.Format),
            fileDownloadName: null,
            enableRangeProcessing: true,
            lastModified: result.Value.LastModified,
            entityTag: entityTag);
    }
}
