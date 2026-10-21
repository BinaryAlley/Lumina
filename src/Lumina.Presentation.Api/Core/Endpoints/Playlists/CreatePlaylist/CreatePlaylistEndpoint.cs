#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Commands.CreatePlaylist;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.CreatePlaylist;

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
        Result<PlaylistResponse> result = await _createPlaylistCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
