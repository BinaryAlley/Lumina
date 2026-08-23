#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Plugins;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.InstallPlugin;

/// <summary>
/// Handler for the command to install a plugin from an uploaded archive.
/// </summary>
public class InstallPluginCommandHandler : ICommandHandler<InstallPluginCommand, Result<Success>>
{
    private readonly IPluginInstaller _pluginInstaller;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<InstallPluginCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginCommandHandler"/> class.
    /// </summary>
    /// <param name="pluginInstaller">Injected service for installing plugins into the plugin storage directory.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public InstallPluginCommandHandler(
        IPluginInstaller pluginInstaller,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<InstallPluginCommand> validator)
    {
        _pluginInstaller = pluginInstaller;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to install a plugin from an uploaded archive.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(InstallPluginCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // only admins can install plugins
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<Success> installResult = await _pluginInstaller.InstallAsync(command.Archive!, command.FileName!, cancellationToken).ConfigureAwait(false);
        if (installResult.IsFailure)
            return installResult.Errors;

        return Result.Success;
    }
}
