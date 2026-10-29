#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Lumina.Application.Common.CQRS;
using Lumina.Application.Core.UsersManagement.Settings.Queries.GetPlaybackState;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;

/// <summary>
/// API endpoint for the <c>/users/me/settings/playback-state</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class GetPlaybackStateEndpoint : BaseEndpoint<EmptyRequest, IResult>
{
    private readonly IQueryHandler<GetPlaybackStateQuery, Result<PlaybackStateResponse>> _getPlaybackStateQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPlaybackStateEndpoint"/> class.
    /// </summary>
    /// <param name="getPlaybackStateQueryHandler">Injected service for handling get playback state queries.</param>
    public GetPlaybackStateEndpoint(IQueryHandler<GetPlaybackStateQuery, Result<PlaybackStateResponse>> getPlaybackStateQueryHandler)
    {
        _getPlaybackStateQueryHandler = getPlaybackStateQueryHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(Http.GET);
        Routes(ApiRoutes.Users.GET_PLAYBACK_STATE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Gets the playback state the player reported for the current user automatically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        Result<PlaybackStateResponse> result = await _getPlaybackStateQueryHandler.HandleAsync(new GetPlaybackStateQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
