#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistScopeEntries;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistScopeEntries;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}/entries-scope</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class AddPlaylistScopeEntriesEndpoint : BaseEndpoint<AddPlaylistScopeEntriesRequest, IResult>
{
    private readonly ICommandHandler<AddPlaylistScopeEntriesCommand, Result<PlaylistEntriesAddedResponse>> _addPlaylistScopeEntriesCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistScopeEntriesEndpoint"/> class.
    /// </summary>
    /// <param name="addPlaylistScopeEntriesCommandHandler">Injected service for handling add playlist scope entries commands.</param>
    public AddPlaylistScopeEntriesEndpoint(ICommandHandler<AddPlaylistScopeEntriesCommand, Result<PlaylistEntriesAddedResponse>> addPlaylistScopeEntriesCommandHandler)
    {
        _addPlaylistScopeEntriesCommandHandler = addPlaylistScopeEntriesCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Playlists.ADD_PLAYLIST_SCOPE_ENTRIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds every track of a music library, an artist or an album to a playlist of the current user.
    /// </summary>
    /// <param name="request">The request containing the scope whose tracks are appended.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddPlaylistScopeEntriesRequest request, CancellationToken cancellationToken)
    {
        // The playlist is identified by the route value, not by any id posted in the body.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        Result<PlaylistEntriesAddedResponse> result = await _addPlaylistScopeEntriesCommandHandler.HandleAsync(request.ToCommand(playlistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
