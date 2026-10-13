#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.SaveMusicAlbum;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-save-album</c> route.
/// </summary>
public class SaveMusicAlbumEndpoint : BaseEndpoint<UpdateMusicAlbumRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicAlbumEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public SaveMusicAlbumEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.PUT);
        Routes(WebRoutes.Music.SAVE_ALBUM);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the music album described by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the edited details of the album.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateMusicAlbumRequest request, CancellationToken cancellationToken)
    {
        // The album is identified by the {artistId} and {albumId} route values, not by any id posted in the body, so that a mismatched id can
        // never redirect the update to another album; all ids are normalized to Guids before they are substituted into the upstream URL, so
        // that a crafted value can never escape its URL segment, and an unparseable id is reported by the API as a missing id.
        string routeArtistId = HttpContext.Request.RouteValues["artistId"]?.ToString() ?? string.Empty;
        string artistId = Guid.TryParse(routeArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string routeAlbumId = HttpContext.Request.RouteValues["albumId"]?.ToString() ?? string.Empty;
        string albumId = Guid.TryParse(routeAlbumId, out Guid parsedAlbumId) ? parsedAlbumId.ToString() : Guid.Empty.ToString();
        string queryLibraryId = HttpContext.Request.Query["libraryId"].ToString();
        string libraryId = Guid.TryParse(queryLibraryId, out Guid parsedLibraryId) ? parsedLibraryId.ToString() : Guid.Empty.ToString();

        MusicAlbumDto album = await _apiHttpClient.PutAsync<MusicAlbumDto, UpdateMusicAlbumRequest>(
            ApiRoutes.Music.UPDATE_ALBUM.Replace("{libraryId}", libraryId).Replace("{artistId}", artistId).Replace("{albumId}", albumId), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(album);
    }
}
