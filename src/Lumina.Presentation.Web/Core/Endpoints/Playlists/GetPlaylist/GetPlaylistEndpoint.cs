#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}/api-get-playlist</c> route.
/// </summary>
public class GetPlaylistEndpoint : BaseEndpoint<GetPlaylistRequest, IResult>
{
    private const int DEFAULT_ENTRY_PAGE_SIZE = 200;

    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public GetPlaylistEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.GET_PLAYLIST);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
    }

    /// <summary>
    /// Retrieves a page of the entries of a playlist of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPlaylistRequest request, CancellationToken cancellationToken)
    {
        // The playlist belongs to the authenticated user, so no intermediary may store it, and the response varies by the sign in cookie.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Cookie";
        // The route value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted value can never escape its URL segment.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        int currentPage = request.CurrentPage ?? 1;
        // The page bounds are normalized by the API pagination data.
        int perPage = request.PerPage ?? DEFAULT_ENTRY_PAGE_SIZE;
        string url = ApiRoutes.Playlists.GET_PLAYLIST.Replace("{playlistId}", playlistId) + $"?currentPage={currentPage}&perPage={perPage}";
        PlaylistDto playlist = await _apiHttpClient.GetAsync<PlaylistDto>(url, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(playlist);
    }
}
