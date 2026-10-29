#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.UpdatePlaylist;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.UpdatePlaylist;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class UpdatePlaylistEndpoint : BaseEndpoint<UpdatePlaylistRequest, IResult>
{
    private readonly ICommandHandler<UpdatePlaylistCommand, Result<PlaylistDetailsResponse>> _updatePlaylistCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="updatePlaylistCommandHandler">Injected service for handling update playlist commands.</param>
    public UpdatePlaylistEndpoint(ICommandHandler<UpdatePlaylistCommand, Result<PlaylistDetailsResponse>> updatePlaylistCommandHandler)
    {
        _updatePlaylistCommandHandler = updatePlaylistCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Playlists.UPDATE_PLAYLIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the details of a playlist of the current user.
    /// </summary>
    /// <param name="request">The request containing the new details of the playlist.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdatePlaylistRequest request, CancellationToken cancellationToken)
    {
        // The playlists belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        // The playlist is identified by the route value, not by any id posted in the body, so that a mismatched id can never redirect the
        // update to another playlist.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        Result<PlaylistDetailsResponse> result = await _updatePlaylistCommandHandler.HandleAsync(request.ToCommand(playlistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
