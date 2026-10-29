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

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/item/{playlistId}/entries/{playlistEntryId}/api-remove-entry</c> route.
/// </summary>
public class RemovePlaylistEntryEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public RemovePlaylistEntryEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.REMOVE_PLAYLIST_ENTRY);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Removes an entry from a playlist of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        // The route values are normalized to Guids before they are substituted into the upstream URL, so that a crafted value can never escape its URL segment.
        string routePlaylistId = HttpContext.Request.RouteValues["playlistId"]?.ToString() ?? string.Empty;
        string playlistId = Guid.TryParse(routePlaylistId, out Guid parsedPlaylistId) ? parsedPlaylistId.ToString() : Guid.Empty.ToString();
        string routePlaylistEntryId = HttpContext.Request.RouteValues["playlistEntryId"]?.ToString() ?? string.Empty;
        string playlistEntryId = Guid.TryParse(routePlaylistEntryId, out Guid parsedPlaylistEntryId) ? parsedPlaylistEntryId.ToString() : Guid.Empty.ToString();
        await _apiHttpClient.DeleteAsync(
            ApiRoutes.Playlists.REMOVE_PLAYLIST_ENTRY
                .Replace("{playlistId}", playlistId)
                .Replace("{playlistEntryId}", playlistEntryId),
            cancellationToken).ConfigureAwait(false);
        return JsonSuccess();
    }
}
