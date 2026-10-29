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

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}/api-update-playlist</c> route.
/// </summary>
public class UpdatePlaylistEndpoint : BaseEndpoint<UpdatePlaylistRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public UpdatePlaylistEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.UPDATE_PLAYLIST);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Updates the details of a playlist of the current user.
    /// </summary>
    /// <param name="request">The request containing the new details of the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdatePlaylistRequest request, CancellationToken cancellationToken)
    {
        // The playlist is identified by the route value, not by any id posted in the body.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        PlaylistDetailsDto playlist = await _apiHttpClient
            .PutAsync<PlaylistDetailsDto, UpdatePlaylistRequest>(ApiRoutes.Playlists.UPDATE_PLAYLIST.Replace("{playlistId}", playlistId), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(playlist);
    }
}
