#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.Playlists.Queries.GetPlaylists;
using Lumina.Contracts.Responses.Playlists;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.Playlists;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.Playlists.GetPlaylists;

/// <summary>
/// API endpoint for the <c>/users/me/playlists</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetPlaylistsEndpoint : BaseEndpoint<FastEndpoints.EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetPlaylistsQuery, Result<PlaylistSummaryResponse[]>> _getPlaylistsQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaylistsEndpoint"/> class.
    /// </summary>
    /// <param name="getPlaylistsQueryHandler">Injected service for handling get playlists queries.</param>
    public GetPlaylistsEndpoint(IQueryHandler<GetPlaylistsQuery, Result<PlaylistSummaryResponse[]>> getPlaylistsQueryHandler)
    {
        _getPlaylistsQueryHandler = getPlaylistsQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.GET);
        Routes(ApiRoutes.Playlists.GET_PLAYLISTS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the playlists of the current user.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(FastEndpoints.EmptyRequest request, CancellationToken cancellationToken)
    {
        // The playlists belong to the authenticated user, so no intermediary may store any response of this route, including its problem
        // responses, and every response varies by the bearer token that authorized it.
        HttpContext.Response.Headers["Cache-Control"] = "private, no-store";
        HttpContext.Response.Headers["Vary"] = "Authorization";
        Result<PlaylistSummaryResponse[]> result = await _getPlaylistsQueryHandler.HandleAsync(new GetPlaylistsQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
