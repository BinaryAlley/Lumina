#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.Exceptions;
using Lumina.Presentation.Web.Common.Playback;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetPlaybackManifest;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-playback-manifest</c> route.
/// </summary>
public class GetPlaybackManifestEndpoint : BaseEndpoint<GetPlaybackManifestRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackManifestEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetPlaybackManifestEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.GET);
        Routes(WebRoutes.Music.GET_PLAYBACK_MANIFEST);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Retrieves the playback manifest of the track identified by <paramref name="request"/>, with the stream path of this application.
    /// </summary>
    /// <param name="request">The request containing the Ids of the artist, album, track and media library to get the manifest of, and the capabilities of the player.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPlaybackManifestRequest request, CancellationToken cancellationToken)
    {
        // The route values are normalized to Guids before they are substituted into the upstream URL, so that a crafted route value can never
        // escape its URL segment, and an unparseable id is reported by the API as a missing id.
        string normalizedArtistId = Guid.TryParse(request.ArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string normalizedAlbumId = Guid.TryParse(request.AlbumId, out Guid parsedAlbumId) ? parsedAlbumId.ToString() : Guid.Empty.ToString();
        string normalizedTrackId = Guid.TryParse(request.TrackId, out Guid parsedTrackId) ? parsedTrackId.ToString() : Guid.Empty.ToString();
        string endpoint = ApiRoutes.Music.GET_PLAYBACK_MANIFEST
            .Replace("{libraryId}", request.LibraryId.ToString())
            .Replace("{artistId}", normalizedArtistId)
            .Replace("{albumId}", normalizedAlbumId)
            .Replace("{trackId}", normalizedTrackId);
        // The capabilities the player advertises are optional and are forwarded only when the player sent them.
        if (!string.IsNullOrWhiteSpace(request.Capabilities))
            endpoint = $"{endpoint}?capabilities={Uri.EscapeDataString(request.Capabilities)}";

        PlaybackManifestDto? manifest = await _apiHttpClient.GetAsync<PlaybackManifestDto>(endpoint, cancellationToken).ConfigureAwait(false) ?? throw new ApiException(new ProblemDetailsDto
            {
                Title = "General.Failure",
                Detail = "InternalServerError",
                Status = StatusCodes.Status502BadGateway
            }, HttpStatusCode.BadGateway, endpoint);
        // The API only knows the relative paths of its own stream and segment endpoints, and the browser only reaches this application, so
        // they are replaced with the paths of the streaming proxies of this application, which are the URLs the player actually consumes.
        manifest.StreamPath = BuildBrowserStreamPath(request.LibraryId, normalizedArtistId, normalizedAlbumId, normalizedTrackId);
        if (string.IsNullOrEmpty(manifest.SegmentStreamPathTemplate))
        {
            manifest.DeliveryId = null;
            manifest.SegmentStreamPathTemplate = null;
        }
        else
        {
            // The API decides how its delivery is addressed: the web app only rewrites the path it reported, and only requires the opaque
            // handle the path needs. It never infers the addressing from the direct play flag.
            if (!PlaybackDeliveryId.IsValid(manifest.DeliveryId))
                throw new ApiException(new ProblemDetailsDto
                {
                    Title = "General.Failure",
                    Detail = "InternalServerError",
                    Status = StatusCodes.Status502BadGateway
                }, HttpStatusCode.BadGateway, endpoint);
            manifest.SegmentStreamPathTemplate = BuildBrowserSegmentStreamPath(request.LibraryId, normalizedArtistId, normalizedAlbumId, normalizedTrackId);
        }
        // The manifest is specific to the authenticated user and carries a stateful delivery handle, so the browser hop never stores it publicly and
        // it varies by the cookie that authenticates this hop.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        return JsonSuccess(manifest);
    }

    /// <summary>
    /// Builds the browser facing relative path of the streaming proxy of this application for the track identified by the given Ids.
    /// </summary>
    /// <remarks>
    /// The path is built from the route template of this application, and never from the host, the scheme, or the port of the incoming request.
    /// The path base is derived from a forwarded header that a caller can spoof, so it is only used when it is a plain absolute path, and the
    /// culture is confined to a single path segment, so that no part of the result can introduce an authority, which would make the browser
    /// resolve the stream from another origin.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The normalized Id of the artist the album belongs to.</param>
    /// <param name="albumId">The normalized Id of the album the track belongs to.</param>
    /// <param name="trackId">The normalized Id of the track.</param>
    /// <returns>The root relative path the browser fetches the stream from, on its own origin.</returns>
    private string BuildBrowserStreamPath(Guid libraryId, string artistId, string albumId, string trackId)
    {
        string culture = IsSafePathSegment(Culture) ? Culture : "en-US";
        string pathBase = IsSafePathBase(HttpContext.Request.PathBase.ToString()) ? HttpContext.Request.PathBase.ToString().TrimEnd('/') : string.Empty;
        string streamPath = WebRoutes.Music.STREAM_TRACK
            .Replace("{culture}", culture, StringComparison.OrdinalIgnoreCase)
            .Replace("{artistId}", artistId, StringComparison.Ordinal)
            .Replace("{albumId}", albumId, StringComparison.Ordinal)
            .Replace("{trackId}", trackId, StringComparison.Ordinal);
        return $"{pathBase}/{streamPath}?libraryId={libraryId}";
    }

    /// <summary>
    /// Builds the browser facing relative path template of the segment streaming proxy of this application for the track identified by the given Ids.
    /// </summary>
    /// <remarks>
    /// The placeholders of the route template are replaced with the Ids the manifest was requested for, and the delivery handle and segment index
    /// placeholders are kept for the player to expand. The player substitutes the handle the manifest reported, so the segment request addresses
    /// exactly the delivery that manifest declared, and the path is built exactly like the track stream path, so it can only ever point at this origin.
    /// </remarks>
    /// <param name="libraryId">The Id of the media library the track belongs to.</param>
    /// <param name="artistId">The normalized Id of the artist the album belongs to.</param>
    /// <param name="albumId">The normalized Id of the album the track belongs to.</param>
    /// <param name="trackId">The normalized Id of the track.</param>
    /// <returns>The root relative path template the browser fetches a segment from, on its own origin.</returns>
    private string BuildBrowserSegmentStreamPath(Guid libraryId, string artistId, string albumId, string trackId)
    {
        string culture = IsSafePathSegment(Culture) ? Culture : "en-US";
        string pathBase = IsSafePathBase(HttpContext.Request.PathBase.ToString()) ? HttpContext.Request.PathBase.ToString().TrimEnd('/') : string.Empty;
        string segmentPath = WebRoutes.Music.STREAM_TRACK_SEGMENT
            .Replace("{culture}", culture, StringComparison.OrdinalIgnoreCase)
            .Replace("{artistId}", artistId, StringComparison.Ordinal)
            .Replace("{albumId}", albumId, StringComparison.Ordinal)
            .Replace("{trackId}", trackId, StringComparison.Ordinal);
        return $"{pathBase}/{segmentPath}?libraryId={libraryId}";
    }

    /// <summary>
    /// Checks whether a value can be used safely as a single segment of a path the browser resolves against its own origin.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> when the value cannot introduce a path separator, a query, a fragment, or a control character, <see langword="false"/> otherwise.</returns>
    private static bool IsSafePathSegment(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        foreach (char character in value)
            if (character == '/' || character == '\\' || character == '?' || character == '#' || character == '%' || char.IsControl(character))
                return false;
        return true;
    }

    /// <summary>
    /// Checks whether a value can be used safely as the path base of a path the browser resolves against its own origin.
    /// </summary>
    /// <param name="pathBase">The path base to check.</param>
    /// <returns><see langword="true"/> when the value is a plain absolute path, <see langword="false"/> otherwise.</returns>
    private static bool IsSafePathBase(string pathBase)
    {
        if (string.IsNullOrEmpty(pathBase))
            return true;
        // A path base that does not start with a single slash, or that contains a backslash or a control character, can be normalized by a
        // browser into a protocol relative or absolute reference, which would point the stream fetch at another origin. A query or a fragment
        // (or an escape that decodes into one) would truncate the built path instead.
        if (!pathBase.StartsWith('/') || pathBase.StartsWith("//", StringComparison.Ordinal))
            return false;
        foreach (char character in pathBase)
            if (character == '\\' || character == '?' || character == '#' || character == '%' || char.IsControl(character))
                return false;
        return !pathBase.Contains("//", StringComparison.Ordinal);
    }
}
