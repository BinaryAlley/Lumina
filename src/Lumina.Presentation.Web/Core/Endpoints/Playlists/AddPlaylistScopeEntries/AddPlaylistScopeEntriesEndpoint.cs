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

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.AddPlaylistScopeEntries;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}/api-add-scope-entries</c> route.
/// </summary>
public class AddPlaylistScopeEntriesEndpoint : BaseEndpoint<AddPlaylistScopeEntriesRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public AddPlaylistScopeEntriesEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.ADD_PLAYLIST_SCOPE_ENTRIES);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Adds every track of a music library, an artist or an album to a playlist of the current user.
    /// </summary>
    /// <param name="request">The request containing the scope whose tracks are appended.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddPlaylistScopeEntriesRequest request, CancellationToken cancellationToken)
    {
        // The playlist is identified by the route value, not by any id posted in the body.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        PlaylistEntriesAddedDto result = await _apiHttpClient
            .PostAsync<PlaylistEntriesAddedDto, AddPlaylistScopeEntriesRequest>(ApiRoutes.Playlists.ADD_PLAYLIST_SCOPE_ENTRIES.Replace("{playlistId}", playlistId), request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(result);
    }
}
