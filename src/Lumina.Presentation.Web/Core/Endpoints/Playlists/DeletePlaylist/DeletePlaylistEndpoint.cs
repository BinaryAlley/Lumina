#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.DeletePlaylist;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}/api-delete-playlist</c> route.
/// </summary>
public class DeletePlaylistEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public DeletePlaylistEndpoint(IApiHttpClient apiHttpClient)
    {
        _apiHttpClient = apiHttpClient;
    }

    /// <summary>
    /// Configures the endpoint.
    /// </summary>
    public override void Configure()
    {
        base.Configure();
        Verbs(Http.DELETE);
        Routes(WebRoutes.Playlists.DELETE_PLAYLIST);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Deletes a playlist of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The route value is normalized to a Guid before it is substituted into the upstream URL, so that a crafted value can never escape its URL segment.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        await _apiHttpClient.DeleteAsync(ApiRoutes.Playlists.DELETE_PLAYLIST.Replace("{playlistId}", playlistId), cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
