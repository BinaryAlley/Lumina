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

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}/api-add-entries</c> route.
/// </summary>
public class AddPlaylistEntriesEndpoint : BaseEndpoint<AddPlaylistEntriesRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public AddPlaylistEntriesEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.ADD_PLAYLIST_ENTRIES);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Adds several tracks to a playlist of the current user, in order.
    /// </summary>
    /// <param name="request">The request containing the tracks to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddPlaylistEntriesRequest request, CancellationToken cancellationToken)
    {
        // The playlist is identified by the route value, not by any id posted in the body.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        PlaylistEntriesAddedDto playlist = await _apiHttpClient
            .PostAsync<PlaylistEntriesAddedDto, AddPlaylistEntriesRequest>(ApiRoutes.Playlists.ADD_PLAYLIST_ENTRIES.Replace("{playlistId}", playlistId), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(playlist);
    }
}
