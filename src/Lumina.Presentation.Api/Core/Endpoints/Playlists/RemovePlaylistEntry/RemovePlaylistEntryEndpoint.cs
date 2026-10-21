#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Commands.RemovePlaylistEntry;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.RemovePlaylistEntry;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}/entries/{playlistEntryId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class RemovePlaylistEntryEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly ICommandHandler<RemovePlaylistEntryCommand, Result<PlaylistEntryRemovedResponse>> _removePlaylistEntryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePlaylistEntryEndpoint"/> class.
    /// </summary>
    /// <param name="removePlaylistEntryCommandHandler">Injected service for handling remove playlist entry commands.</param>
    public RemovePlaylistEntryEndpoint(ICommandHandler<RemovePlaylistEntryCommand, Result<PlaylistEntryRemovedResponse>> removePlaylistEntryCommandHandler)
    {
        _removePlaylistEntryCommandHandler = removePlaylistEntryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.DELETE);
        Routes(ApiRoutes.Playlists.REMOVE_PLAYLIST_ENTRY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Removes an entry from a playlist of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // Take the unique identifiers from the route.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        string? playlistEntryId = HttpContext.Request.RouteValues["playlistEntryId"]?.ToString();
        Result<PlaylistEntryRemovedResponse> result = await _removePlaylistEntryCommandHandler.HandleAsync(new RemovePlaylistEntryCommand(playlistId, playlistEntryId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
