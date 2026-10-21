#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.MovePlaylistEntry;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.MovePlaylistEntry;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}/entries/{playlistEntryId}/move</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class MovePlaylistEntryEndpoint : BaseEndpoint<MovePlaylistEntryRequest, IResult>
{
    private readonly ICommandHandler<MovePlaylistEntryCommand, Result<PlaylistEntryMovedResponse>> _movePlaylistEntryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MovePlaylistEntryEndpoint"/> class.
    /// </summary>
    /// <param name="movePlaylistEntryCommandHandler">Injected service for handling move playlist entry commands.</param>
    public MovePlaylistEntryEndpoint(ICommandHandler<MovePlaylistEntryCommand, Result<PlaylistEntryMovedResponse>> movePlaylistEntryCommandHandler)
    {
        _movePlaylistEntryCommandHandler = movePlaylistEntryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Playlists.MOVE_PLAYLIST_ENTRY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Moves one entry of a playlist of the current user to a new position.
    /// </summary>
    /// <param name="request">The request containing the new position of the entry.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(MovePlaylistEntryRequest request, CancellationToken cancellationToken)
    {
        // The playlist and the entry are identified by the route values, not by any id posted in the body.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        string? playlistEntryId = HttpContext.Request.RouteValues["playlistEntryId"]?.ToString();
        Result<PlaylistEntryMovedResponse> result = await _movePlaylistEntryCommandHandler.HandleAsync(request.ToCommand(playlistId, playlistEntryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
