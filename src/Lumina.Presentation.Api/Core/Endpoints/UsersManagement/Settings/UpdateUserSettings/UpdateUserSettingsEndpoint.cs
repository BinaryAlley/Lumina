#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Mapping.UsersManagement.Users;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdateUserSettings;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using Lumina.Domain.Common.Primitives;
using Lumina.Presentation.Api.Common.Routes.UsersManagement;
using Lumina.Presentation.Api.Core.Endpoints.Common;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.Core.Endpoints.UsersManagement.Settings.UpdateUserSettings;

/// <summary>
/// API endpoint for the <c>/users/me/settings</c> route.
/// </summary>
/// <remarks>
/// Uses "me" because it is always for the currently authenticated user.
/// </remarks>
public class UpdateUserSettingsEndpoint : BaseEndpoint<UpdateUserSettingsRequest, IResult>
{
    private readonly ICommandHandler<UpdateUserSettingsCommand, Result<Updated>> _updateUserSettingsCommandHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSettingsEndpoint"/> class.
    /// </summary>
    /// <param name="updateUserSettingsCommandHandler">Injected service for handling update user settings commands.</param>
    public UpdateUserSettingsEndpoint(ICommandHandler<UpdateUserSettingsCommand, Result<Updated>> updateUserSettingsCommandHandler)
    {
        _updateUserSettingsCommandHandler = updateUserSettingsCommandHandler;
    }

    /// <summary>
    /// Configures the API endpoint.
    /// </summary>
    public override void Configure()
    {
        Verbs(FastEndpoints.Http.PUT);
        Routes(ApiRoutes.Users.UPDATE_USER_SETTINGS);
        Version(1);
        DontCatchExceptions();
    }

    /// <summary>
    /// Updates the settings of the current user.
    /// </summary>
    /// <param name="request">The request containing the new settings of the current user.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    public override async Task<IResult> ExecuteAsync(UpdateUserSettingsRequest request, CancellationToken cancellationToken)
    {
        Result<Updated> result = await _updateUserSettingsCommandHandler.HandleAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        return result.Match(success => TypedResults.Ok(success), Problem);
    }
}
