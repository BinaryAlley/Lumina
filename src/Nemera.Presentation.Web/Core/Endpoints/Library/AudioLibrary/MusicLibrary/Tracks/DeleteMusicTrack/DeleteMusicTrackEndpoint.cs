#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.DeleteMusicTrack;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-delete-track</c> route.
/// </summary>
public class DeleteMusicTrackEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMusicTrackEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DeleteMusicTrackEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.DELETE);
        Routes(WebRoutes.Music.DELETE_TRACK);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes the music track identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The track is identified by the {artistId}, {albumId} and {trackId} route values; all ids are normalized to Guids before they are substituted
        // into the upstream URL, so that a crafted value can never escape its URL segment, and an unparseable id is reported by the API as a missing id.
        string routeArtistId = HttpContext.Request.RouteValues["artistId"]?.ToString() ?? string.Empty;
        string artistId = Guid.TryParse(routeArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string routeAlbumId = HttpContext.Request.RouteValues["albumId"]?.ToString() ?? string.Empty;
        string albumId = Guid.TryParse(routeAlbumId, out Guid parsedAlbumId) ? parsedAlbumId.ToString() : Guid.Empty.ToString();
        string routeTrackId = HttpContext.Request.RouteValues["trackId"]?.ToString() ?? string.Empty;
        string trackId = Guid.TryParse(routeTrackId, out Guid parsedTrackId) ? parsedTrackId.ToString() : Guid.Empty.ToString();
        string queryLibraryId = HttpContext.Request.Query["libraryId"].ToString();
        string libraryId = Guid.TryParse(queryLibraryId, out Guid parsedLibraryId) ? parsedLibraryId.ToString() : Guid.Empty.ToString();

        await _apiHttpClient.DeleteAsync(ApiRoutes.Music.DELETE_TRACK.Replace("{libraryId}", libraryId).Replace("{artistId}", artistId).Replace("{albumId}", albumId).Replace("{trackId}", trackId), cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
