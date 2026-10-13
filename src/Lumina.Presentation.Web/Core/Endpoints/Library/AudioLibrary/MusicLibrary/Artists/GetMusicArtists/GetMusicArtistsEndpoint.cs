#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Library.AudioLibrary.MusicLibrary.Artists.GetMusicArtists;

/// <summary>
/// API endpoint for the <c>/{culture}/library/audio-library/music-library/artists/api-get-artists</c> route.
/// </summary>
public class GetMusicArtistsEndpoint : BaseEndpoint<GetMusicArtistsLiteRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMusicArtistsEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetMusicArtistsEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Music.GET_ARTISTS);
        DontAutoTag();
        Options(options => options.WithTags("Music"));
    }

    /// <summary>
    /// Retrieves the lightweight details of the music artists of a media library.
    /// </summary>
    /// <param name="request">The request containing the parameters used to retrieve the artists.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetMusicArtistsLiteRequest request, CancellationToken cancellationToken)
    {
        PaginatedMusicArtistLiteDto response = await _apiHttpClient.GetAsync<PaginatedMusicArtistLiteDto>(BuildItemsEndpoint(request), cancellationToken).ConfigureAwait(false);
        return JsonSuccess(response);
    }

    /// <summary>
    /// Builds the API endpoint used to retrieve the lightweight details of the music artists of a media library.
    /// </summary>
    /// <param name="query">The request containing the parameters used to retrieve the artists.</param>
    /// <returns>The API endpoint to which the retrieval request is sent.</returns>
    private static string BuildItemsEndpoint(GetMusicArtistsLiteRequest query)
    {
        StringBuilder endpoint = new(ApiRoutes.Music.GET_ARTISTS_LITE.Replace("{libraryId}", query.LibraryId.ToString()));
        List<string> parameters = [];
        if (query.CurrentPage is not null)
            parameters.Add($"currentPage={query.CurrentPage}");
        if (query.PerPage is not null)
            parameters.Add($"perPage={query.PerPage}");
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            parameters.Add($"searchTerm={Uri.EscapeDataString(query.SearchTerm)}");
        if (!string.IsNullOrWhiteSpace(query.FilterAlphaKey))
            parameters.Add($"filterAlphaKey={Uri.EscapeDataString(query.FilterAlphaKey)}");
        parameters.Add($"shouldIgnoreThePrefixForAlphaPicker={query.ShouldIgnoreThePrefixForAlphaPicker}");
        if (!string.IsNullOrWhiteSpace(query.SortBy))
            parameters.Add($"sortBy={Uri.EscapeDataString(query.SortBy)}");
        if (query.SortOrder is not null)
            parameters.Add($"sortOrder={query.SortOrder}");
        if (parameters.Count > 0)
            endpoint.Append('?').Append(string.Join('&', parameters));
        return endpoint.ToString();
    }
}
