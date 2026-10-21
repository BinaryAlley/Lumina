#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntries;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntries;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}/entries-batch</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class AddPlaylistEntriesEndpoint : BaseEndpoint<AddPlaylistEntriesRequest, IResult>
{
    private readonly ICommandHandler<AddPlaylistEntriesCommand, Result<PlaylistEntriesAddedResponse>> _addPlaylistEntriesCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntriesEndpoint"/> class.
    /// </summary>
    /// <param name="addPlaylistEntriesCommandHandler">Injected service for handling add playlist entries commands.</param>
    public AddPlaylistEntriesEndpoint(ICommandHandler<AddPlaylistEntriesCommand, Result<PlaylistEntriesAddedResponse>> addPlaylistEntriesCommandHandler)
    {
        _addPlaylistEntriesCommandHandler = addPlaylistEntriesCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Playlists.ADD_PLAYLIST_ENTRIES);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds several tracks to the end of the ordered entries of a playlist of the current user, in order.
    /// </summary>
    /// <param name="request">The request containing the tracks to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddPlaylistEntriesRequest request, CancellationToken cancellationToken)
    {
        // The playlist is identified by the route value, not by any id posted in the body.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        Result<PlaylistEntriesAddedResponse> result = await _addPlaylistEntriesCommandHandler.HandleAsync(request.ToCommand(playlistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
