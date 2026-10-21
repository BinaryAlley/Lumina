#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Presentation.Web.Common.Api;
using Lumina.Presentation.Web.Common.DTO.Playlists;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using Lumina.Presentation.Web.Common.Routes;
using Lumina.Presentation.Web.Core.Endpoints.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Web.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// API endpoint for the <c>/{culture}/playlists/api-create-playlist</c> route.
/// </summary>
public class CreatePlaylistEndpoint : BaseEndpoint<CreatePlaylistRequest, IResult>
{
    private readonly IApiHttpClient _apiHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="apiHttpClient">Injected service for interactions with the API.</param>
    public CreatePlaylistEndpoint(IApiHttpClient apiHttpClient)
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
        Routes(WebRoutes.Playlists.CREATE_PLAYLIST);
        DontAutoTag();
        Options(options => options.WithTags("Playlists"));
        EnableAntiforgery();
    }

    /// <summary>
    /// Creates a playlist of the current user.
    /// </summary>
    /// <param name="request">The request containing the details of the playlist to create.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CreatePlaylistRequest request, CancellationToken cancellationToken)
    {
        PlaylistDto playlist = await _apiHttpClient.PostAsync<PlaylistDto, CreatePlaylistRequest>(ApiRoutes.Playlists.CREATE_PLAYLIST, request, cancellationToken).ConfigureAwait(false);
        return JsonSuccess(playlist);
    }
}
