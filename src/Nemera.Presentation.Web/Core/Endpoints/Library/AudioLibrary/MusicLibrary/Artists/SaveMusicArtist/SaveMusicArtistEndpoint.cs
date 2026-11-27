#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.SaveMusicArtist;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/api-save-artist</c> route.
/// </summary>
public class SaveMusicArtistEndpoint : BaseEndpoint<UpdateMusicArtistRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMusicArtistEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public SaveMusicArtistEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Music.SAVE_ARTIST);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the music artist described by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the edited details of the artist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateMusicArtistRequest request, CancellationToken cancellationToken)
    {
        // The artist is identified by the {artistId} route value, not by any id posted in the body, so that a mismatched id can never redirect
        // the update to another artist; both ids are normalized to Guids before they are substituted into the upstream URL, so that a crafted
        // value can never escape its URL segment, and an unparseable id is reported by the API as a missing id.
        string routeArtistId = HttpContext.Request.RouteValues["artistId"]?.ToString() ?? string.Empty;
        string artistId = Guid.TryParse(routeArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string queryLibraryId = HttpContext.Request.Query["libraryId"].ToString();
        string libraryId = Guid.TryParse(queryLibraryId, out Guid parsedLibraryId) ? parsedLibraryId.ToString() : Guid.Empty.ToString();

        MusicArtistDto artist = await _apiHttpClient.PutAsync<MusicArtistDto, UpdateMusicArtistRequest>(
            ApiRoutes.Music.UPDATE_ARTIST.Replace("{libraryId}", libraryId).Replace("{artistId}", artistId), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(artist);
    }
}
