#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.Playlists;
using Lumina.Application.Core.Playlists.Queries.GetPlaylist;
using Lumina.Contracts.Requests.Playlists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylist;

/// <summary>
/// API endpoint for the <c>/users/me/playlists/{playlistId}</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetPlaylistEndpoint : BaseEndpoint<GetPlaylistRequest, IResult>
{
    private readonly IQueryHandler<GetPlaylistQuery, Result<PlaylistResponse>> _getPlaylistQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistEndpoint"/> class.
    /// </summary>
    /// <param name="getPlaylistQueryHandler">Injected service for handling get playlist queries.</param>
    public GetPlaylistEndpoint(IQueryHandler<GetPlaylistQuery, Result<PlaylistResponse>> getPlaylistQueryHandler)
    {
        _getPlaylistQueryHandler = getPlaylistQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Playlists.GET_PLAYLIST);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets a page of the entries of a playlist of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(GetPlaylistRequest request, CancellationToken cancellationToken)
    {
        // The playlist belongs to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        // Take the unique identifier from the route.
        string? playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
        Result<PlaylistResponse> result = await _getPlaylistQueryHandler.HandleAsync(request.ToQuery(playlistId), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
