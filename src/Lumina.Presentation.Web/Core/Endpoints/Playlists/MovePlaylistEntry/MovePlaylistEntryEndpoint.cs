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

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}/entries/{playlistEntryId}/api-move-entry</c> route.
/// </summary>
public class MovePlaylistEntryEndpoint : BaseEndpoint<MovePlaylistEntryRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public MovePlaylistEntryEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.MOVE_PLAYLIST_ENTRY);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Moves one entry of a playlist of the current user to a new position.
    /// </summary>
    /// <param name="request">The request containing the new position of the entry.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(MovePlaylistEntryRequest request, CancellationToken cancellationToken)
    {
        // The playlist and the entry are identified by the route values, not by any id posted in the body.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        string routePlaylistEntryId = HttpContext.Request.RouteValues["playlistEntryId"]?.ToString() ?? string.Empty;
        string playlistEntryId = Guid.TryParse(routePlaylistEntryId, out Guid parsedPlaylistEntryId) ? parsedPlaylistEntryId.ToString() : Guid.Empty.ToString();
        PlaylistEntryMovedDto entry = await _apiHttpClient
            .PutAsync<PlaylistEntryMovedDto, MovePlaylistEntryRequest>(
                ApiRoutes.Playlists.MOVE_PLAYLIST_ENTRY
                    .Replace("{playlistId}", playlistId)
                    .Replace("{playlistEntryId}", playlistEntryId),
                request,
                cancellationToken).ConfigureAwait(false);
        return JsonSuccess(entry);
    }
}
