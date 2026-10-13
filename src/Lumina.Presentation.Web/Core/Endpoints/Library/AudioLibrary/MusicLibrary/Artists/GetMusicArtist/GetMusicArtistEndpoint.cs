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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtist;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/api-get-artist</c> route.
/// </summary>
public class GetMusicArtistEndpoint : BaseEndpoint<GetMusicArtistViewRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetMusicArtistEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Music.GET_ARTIST);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Retrieves the full details of the music artist identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the artist to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMusicArtistViewRequest request, CancellationToken cancellationToken)
    {
        // The route value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted route value can never
        // escape its URL segment, and an unparseable id is reported by the API as a missing artist id.
        request = request with { ArtistId = Guid.TryParse(request.ArtistId, out Guid artistId) ? artistId.ToString() : Guid.Empty.ToString() };

        MusicArtistDto artist = await _apiHttpClient.GetAsync<MusicArtistDto>(ApiRoutes.Music.GET_ARTIST.Replace("{libraryId}", request.LibraryId.ToString()).Replace("{artistId}", request.ArtistId), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(artist);
    }
}
