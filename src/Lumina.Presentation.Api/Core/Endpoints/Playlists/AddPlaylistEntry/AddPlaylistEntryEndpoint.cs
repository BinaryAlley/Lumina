#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.AddPlaylistEntry;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.AddPlaylistEntry;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}/entries</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class AddPlaylistEntryEndpoint : BaseEndpoint<AddPlaylistEntryRequest, IResult>
{
    private readonly ICommandHandler<AddPlaylistEntryCommand, Result<PlaylistEntryAddedResponse>> _addPlaylistEntryCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlaylistEntryEndpoint"/> class.
    /// </summary>
    /// <param name="addPlaylistEntryCommandHandler">Injected service for handling add playlist entry commands.</param>
    public AddPlaylistEntryEndpoint(ICommandHandler<AddPlaylistEntryCommand, Result<PlaylistEntryAddedResponse>> addPlaylistEntryCommandHandler)
    {
        _addPlaylistEntryCommandHandler = addPlaylistEntryCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Playlists.ADD_PLAYLIST_ENTRY);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Adds a track to the end of the ordered entries of a playlist of the current user.
    /// </summary>
    /// <param name="request">The request containing the track to add.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(AddPlaylistEntryRequest request, CancellationToken cancellationToken)
    {
        // The playlists belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        // The playlist is identified by the route value, not by any id posted in the body.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        Result<PlaylistEntryAddedResponse> result = await _addPlaylistEntryCommandHandler.HandleAsync(request.ToCommand(playlistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
