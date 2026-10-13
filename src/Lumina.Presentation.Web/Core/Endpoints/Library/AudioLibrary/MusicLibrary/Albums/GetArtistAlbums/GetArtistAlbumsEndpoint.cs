#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Albums.GetArtistAlbums;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/{artistId}/albums/api-get-albums</c> route.
/// </summary>
public class GetArtistAlbumsEndpoint : BaseEndpoint<GetArtistAlbumsLiteRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetArtistAlbumsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetArtistAlbumsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Music.GET_ARTIST_ALBUMS);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Retrieves the lightweight details of the albums of a music artist.
    /// </summary>
    /// <param name="request">The request containing the parameters used to retrieve the albums.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetArtistAlbumsLiteRequest request, CancellationToken cancellationToken)
    {
        PaginatedMusicAlbumLiteDto response = await _apiHttpClient.GetAsync<PaginatedMusicAlbumLiteDto>(BuildItemsEndpoint(request), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }

    /// <summary>
    /// Builds the API endpoint used to retrieve the lightweight details of the albums of a music artist.
    /// </summary>
    /// <param name="query">The request containing the parameters used to retrieve the albums.</param>
    /// <returns>The API endpoint to which the retrieval request is sent.</returns>
    private static string BuildItemsEndpoint(GetArtistAlbumsLiteRequest query)
    {
        // The route value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted route value can never
        // escape its URL segment, and an unparseable id is reported by the API as a missing artist id.
        string artistId = Guid.TryParse(query.ArtistId, out Guid parsedArtistId) ? parsedArtistId.ToString() : Guid.Empty.ToString();
        StringBuilder endpoint = new(ApiRoutes.Music.GET_ARTIST_ALBUMS_LITE.Replace("{libraryId}", query.LibraryId.ToString()).Replace("{artistId}", artistId));
        List<string> parameters = [];
        if (query.CurrentPage is not null)
            parameters.Add($"currentPage={query.CurrentPage}");
        if (query.PerPage is not null)
            parameters.Add($"perPage={query.PerPage}");
        if (parameters.Count > 0)
            endpoint.Append('?').Append(string.Join('&', parameters));
        return endpoint.ToString();
    }
}
