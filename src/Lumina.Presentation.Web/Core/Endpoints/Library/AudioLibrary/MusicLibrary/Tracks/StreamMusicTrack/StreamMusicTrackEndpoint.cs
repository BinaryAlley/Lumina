#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DependencyInjection;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.StreamMusicTrack;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track</c> route.
/// </summary>
public class StreamMusicTrackEndpoint : BaseEndpoint<StreamMusicTrackRequest, IResult>
{
    private readonly IStreamingApiHttpClient _streamingApiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamMusicTrackEndpoint"/> class.
    /// </summary>
    /// <param name="streamingApiHttpClient">Injected service for streaming interactions with the API.</param>
    public StreamMusicTrackEndpoint(IStreamingApiHttpClient streamingApiHttpClient)
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
        Routes(WebRoutes.Music.STREAM_TRACK);
        DontAutoTag();
        // The stream is bounded per user with a concurrency limit, exactly like the stream route of the API server, so that a single user can
        // never hold an unbounded number of long lived streams open at once and exhaust the connections towards the API.
        Options(options => options
            .WithTags("Music")
            .RequireRateLimiting(PresentationWebLayerServices.TRACK_STREAM_RATE_LIMIT_POLICY));
    }

    /// <summary>
    /// Streams the audio file of the track identified by <paramref name="request"/> through the API, without buffering it.
    /// </summary>
    /// <param name="request">The request containing the Ids of the artist, album, track and media library to stream.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(StreamMusicTrackRequest request, CancellationToken cancellationToken)
    {
        // The route values are normalized to Guids before they are substituted into the upstream URL, so that a crafted route value can never
        // escape its URL segment, and an unparseable id is reported by the API as a missing id.
        string normalizedArtistId = Guid.TryParse(request.ArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string normalizedAlbumId = Guid.TryParse(request.AlbumId, out Guid parsedAlbumId) ? parsedAlbumId.ToString() : Guid.Empty.ToString();
        string normalizedTrackId = Guid.TryParse(request.TrackId, out Guid parsedTrackId) ? parsedTrackId.ToString() : Guid.Empty.ToString();
        string endpoint = ApiRoutes.Music.STREAM_TRACK
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", normalizedArtistId)
            .Replace("{albumId}", normalizedAlbumId)
            .Replace("{trackId}", normalizedTrackId);
        // The range and the conditional headers that revalidate it are forwarded as they are, so that the API, which is the only party that
        // knows the entity tag and the modification time of the file, answers a partial, a resumed, or an unchanged request by itself.
        Dictionary<string, string> forwardHeaders = [];
        AddHeader(forwardHeaders, "Range", HttpContext.Request.Headers.Range);
        AddHeader(forwardHeaders, "If-Range", HttpContext.Request.Headers.IfRange);
        AddHeader(forwardHeaders, "If-None-Match", HttpContext.Request.Headers.IfNoneMatch);
        AddHeader(forwardHeaders, "If-Modified-Since", HttpContext.Request.Headers.IfModifiedSince);

        // The API response is disposed once the copy ends, so that the underlying connection is released as soon as the client stops reading.
        await using (ApiStreamResult streamResult = await _streamingApiHttpClient.GetStreamAsync(endpoint, forwardHeaders, cancellationToken).ConfigureAwait(false))
        {
            HttpContext.Response.StatusCode = (int)streamResult.StatusCode;
            CopyResponseHeaders(HttpContext.Response, streamResult);
            // The caching instructions of the API are forwarded, so that the web hop never silently ignores a caching decision the API made,
            // but the response is always private to the user, and it varies by the cookie that authenticates this hop: the browser never sends
            // the bearer token the API sees.
            HttpContext.Response.Headers["Cache-Control"] = BuildCacheControl(streamResult.CacheControl);
            HttpContext.Response.Headers["Vary"] = "Cookie";
            // The body is copied straight from the API to the client, with no intermediate buffering, and the copy is abandoned when the client disconnects.
            await streamResult.Content.CopyToAsync(HttpContext.Response.Body, cancellationToken).ConfigureAwait(false);
        }
        return Results.Empty;
    }

    /// <summary>
    /// Adds a request header to the set of headers forwarded to the API, unless the incoming request did not carry it.
    /// </summary>
    /// <param name="headers">The set of headers forwarded to the API.</param>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header in the incoming request, which is empty when the header was not sent.</param>
    private static void AddHeader(Dictionary<string, string> headers, string name, StringValues value)
    {
        if (value.Count > 0 && !string.IsNullOrWhiteSpace(value.ToString()))
            headers[name] = value.ToString();
    }

    /// <summary>
    /// Copies the status and resource headers of the streamed API response to the response sent to the browser.
    /// </summary>
    /// <param name="response">The response sent to the browser.</param>
    /// <param name="streamResult">The streamed response of the API.</param>
    private static void CopyResponseHeaders(HttpResponse response, ApiStreamResult streamResult)
    {
        if (!string.IsNullOrEmpty(streamResult.ContentType))
            response.ContentType = streamResult.ContentType;
        if (streamResult.ContentLength.HasValue)
            response.ContentLength = streamResult.ContentLength.Value;
        SetHeader(response, "Content-Range", streamResult.ContentRange);
        SetHeader(response, "Accept-Ranges", streamResult.AcceptRanges);
        SetHeader(response, "ETag", streamResult.ETag);
        SetHeader(response, "Last-Modified", streamResult.LastModified?.ToString("R"));
    }

    /// <summary>
    /// Sets a response header to the given value, unless the value is missing.
    /// </summary>
    /// <param name="response">The response to set the header on.</param>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header, or <see langword="null"/> when the API did not send it.</param>
    private static void SetHeader(HttpResponse response, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            response.Headers[name] = value;
    }

    /// <summary>
    /// Builds the caching instructions of the response sent to the browser from the instructions of the API.
    /// </summary>
    /// <remarks>
    /// A stream is specific to the authenticated user, so it must never be stored by a shared cache. Every other instruction of the API,
    /// like a maximum age for the private cache of the browser or a no-store, is kept, so that the web hop never silently ignores a caching
    /// decision the API made for the resource.
    /// </remarks>
    /// <param name="apiCacheControl">The caching instructions of the API response, or <see langword="null"/> when it declared none.</param>
    /// <returns>The caching instructions of the response sent to the browser.</returns>
    private static string BuildCacheControl(string? apiCacheControl)
    {
        if (!CacheControlHeaderValue.TryParse(apiCacheControl, out CacheControlHeaderValue? cacheControl))
            cacheControl = new CacheControlHeaderValue();
        cacheControl.Public = false;
        cacheControl.Private = true;
        return cacheControl.ToString();
    }
}
