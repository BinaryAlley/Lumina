#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdatePlaybackState;

/// <summary>
/// API endpoint for the <c>/users/me/settings/playback-state</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class UpdatePlaybackStateEndpoint : BaseEndpoint<UpdatePlaybackStateRequest, IResult>
{
    private readonly ICommandHandler<UpdatePlaybackStateCommand, Result<Updated>> _updatePlaybackStateCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePlaybackStateEndpoint"/> class.
    /// </summary>
    /// <param name="updatePlaybackStateCommandHandler">Injected service for handling update playback state commands.</param>
    public UpdatePlaybackStateEndpoint(ICommandHandler<UpdatePlaybackStateCommand, Result<Updated>> updatePlaybackStateCommandHandler)
    {
        _updatePlaybackStateCommandHandler = updatePlaybackStateCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Users.UPDATE_PLAYBACK_STATE);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the playback state the player reported for the current user automatically.
    /// </summary>
    /// <param name="request">The request containing the latest playback state of the current user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdatePlaybackStateRequest request, CancellationToken cancellationToken)
    {
        Result<Updated> result = await _updatePlaybackStateCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
