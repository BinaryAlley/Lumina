#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Tracks.GetMusicTrack;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-track</c> route.
/// </summary>
public class GetMusicTrackEndpoint : BaseEndpoint<GetMusicTrackViewRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicTrackEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetMusicTrackEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Music.GET_TRACK);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Retrieves the full details of the music track identified by <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The request containing the Ids of the artist, album, track and media library to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMusicTrackViewRequest request, CancellationToken cancellationToken)
    {
        // The route values are normalized to Guids before they are substituted into the upstream URL, so that a crafted route value can never
        // escape its URL segment, and an unparseable id is reported by the API as a missing id.
        string normalizedArtistId = Guid.TryParse(request.ArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        string normalizedAlbumId = Guid.TryParse(request.AlbumId, out Guid parsedAlbumId) ? parsedAlbumId.ToString() : Guid.Empty.ToString();
        string normalizedTrackId = Guid.TryParse(request.TrackId, out Guid parsedTrackId) ? parsedTrackId.ToString() : Guid.Empty.ToString();

        MusicTrackDto track = await _apiHttpClient.GetAsync<MusicTrackDto>(
            ApiRoutes.Music.GET_TRACK.Replace("{libraryId}", request.LibraryId.ToString()).Replace("{artistId}", normalizedArtistId).Replace("{albumId}", normalizedAlbumId).Replace("{trackId}", normalizedTrackId), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(track);
    }
}
