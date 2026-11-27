#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Presentation.Web.Common.Api;
using Nemera.Presentation.Web.Common.Routes;
using Nemera.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.RestoreMusicArtist;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/api-restore-artist</c> route.
/// </summary>
public class RestoreMusicArtistEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreMusicArtistEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public RestoreMusicArtistEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.POST);
        Routes(WebRoutes.Music.RESTORE_ARTIST);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Restores the deleted music artist identified by the route.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The artist is identified by the {artistId} route value and the owning library by the {libraryId} query string; both ids are
        // normalized to Guids before they are substituted into the upstream URL, so that a crafted value can never escape its URL segment.
        string routeArtistId = HttpContext.Request.RouteValues["artistId"]?.ToString() ?? string.Empty;
        string artistId = Guid.TryParse(routeArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string queryLibraryId = HttpContext.Request.Query["libraryId"].ToString();
        string libraryId = Guid.TryParse(queryLibraryId, out Guid parsedLibraryId) ? parsedLibraryId.ToString() : Guid.Empty.ToString();

        await _apiHttpClient.PostIgnoringResponseAsync(
            ApiRoutes.Music.RESTORE_ARTIST.Replace("{libraryId}", libraryId).Replace("{artistId}", artistId),
            cancellationToken).ConfigureAwait(false);
        return JsonSuccess(new { isRestored = true });
    }
}
