#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Commands.DeletePlaylist;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.DeletePlaylist;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class DeletePlaylistEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<DeletePlaylistCommand, Result<Deleted>> _deletePlaylistCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="deletePlaylistCommandHandler">Injected service for handling delete playlist commands.</param>
    public DeletePlaylistEndpoint(ICommandHandler<DeletePlaylistCommand, Result<Deleted>> deletePlaylistCommandHandler)
    {
        _deletePlaylistCommandHandler = deletePlaylistCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Playlists.DELETE_PLAYLIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Deletes a playlist of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifier from the route.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        Result<Deleted> result = await _deletePlaylistCommandHandler.HandleAsync(new DeletePlaylistCommand(playlistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
