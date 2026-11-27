#region ========================================================================= USING =====================================================================================
using FastEndpoints;
using Nemera.Application.Common.CQRS;
using Nemera.Application.Core.UsersManagement.Settings.Queries.GetPlaybackState;
using Nemera.Contracts.Responses.UsersManagement.Settings;
using Nemera.Domain.Common.Primitives;
using Nemera.Presentation.Api.Common.Routes.UsersManagement;
using Nemera.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.Core.Endpoints.UsersManagement.Settings.GetPlaybackState;

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
    /// <param name="request">The empty request, which carries no data.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        Result<PlaybackStateResponse> result = await _getPlaybackStateQueryHandler.HandleAsync(new GetPlaybackStateQuery(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
