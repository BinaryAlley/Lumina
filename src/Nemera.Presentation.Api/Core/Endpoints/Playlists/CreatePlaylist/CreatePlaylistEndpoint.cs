#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using Nemera.Application.Common.Mapping.Playlists;
using Nemera.Application.Core.Playlists.Commands.CreatePlaylist;
using Nemera.Contracts.Requests.Playlists;
using Nemera.Contracts.Responses.Playlists;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.Playlists;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.Playlists.CreatePlaylist;

/// <summary>
/// API endpoint for the <c>/users/me/playlists</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class CreatePlaylistEndpoint : BaseEndpoint<CreatePlaylistRequest, IResult>
{
    private readonly ICommandHandler<CreatePlaylistCommand, Result<PlaylistResponse>> _createPlaylistCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="createPlaylistCommandHandler">Injected service for handling create playlist commands.</param>
    public CreatePlaylistEndpoint(ICommandHandler<CreatePlaylistCommand, Result<PlaylistResponse>> createPlaylistCommandHandler)
    {
        _createPlaylistCommandHandler = createPlaylistCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.POST);
        Routes(ApiRoutes.Playlists.CREATE_PLAYLIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Creates a playlist of the current user.
    /// </summary>
    /// <param name="request">The request containing the details of the playlist to create.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(CreatePlaylistRequest request, CancellationToken cancellationToken)
    {
        // The playlists belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        Result<PlaylistResponse> result = await _createPlaylistCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
