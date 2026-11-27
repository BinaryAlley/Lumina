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

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetMusicAlbum;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-get-album</c> route.
/// </summary>
public class GetMusicAlbumEndpoint : BaseEndpoint<GetMusicAlbumViewRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicAlbumEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetMusicAlbumEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Music.GET_ALBUM);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Retrieves the full details of the music album identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Id of the album to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMusicAlbumViewRequest request, CancellationToken cancellationToken)
    {
        // The route values are normalized to Guids before they are substituted into the upstream URL, so that a crafted route value can never
        // escape its URL segment, and an unparseable id is reported by the API as a missing album id.
        request = request with { ArtistId = Guid.TryParse(request.ArtistId, out Guid artistId) ? artistId.ToString() : Guid.Empty.ToString() };
        request = request with { AlbumId = Guid.TryParse(request.AlbumId, out Guid albumId) ? albumId.ToString() : Guid.Empty.ToString() };

        MusicAlbumDto album = await _apiHttpClient.GetAsync<MusicAlbumDto>(ApiRoutes.Music.GET_ALBUM.Replace("{libraryId}", request.LibraryId.ToString()).Replace("{artistId}", request.ArtistId).Replace("{albumId}", request.AlbumId), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(album);
    }
}
