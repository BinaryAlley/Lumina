#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;

/// <summary>
/// Handler for the command to restore a soft deleted bundled theme.
/// </summary>
public class RestoreThemeCommandHandler : ICommandHandler<RestoreThemeCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IThemeService _themeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<RestoreThemeCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public RestoreThemeCommandHandler(
        IUnitOfWork unitOfWork,
        IThemeService themeService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<RestoreThemeCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _themeService = themeService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to restore a soft deleted bundled theme.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(RestoreThemeCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // only admins can restore themes
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<ThemeEntity?> getThemeResult = await _unitOfWork.ThemeRepository.GetByThemeIdAsync(command.ThemeId!, cancellationToken).ConfigureAwait(false);
        if (getThemeResult.IsFailure)
            return getThemeResult.Errors;

        ThemeEntity? theme = getThemeResult.Value;
        if (theme is null)
            return DomainErrors.Themes.ThemeNotFound;

        // only bundled themes are soft deleted, so only a soft deleted bundled theme can be restored
        if (!theme.IsDeleted || theme.InstallSource != ThemeInstallSource.Bundled)
            return DomainErrors.Themes.ThemeCannotBeRestored;

        // restore the pack files from the shipped archive before reactivating the theme
        Result<Success> restoreFilesResult = await _themeService.RestoreBundledThemeAsync(theme.ThemeId, cancellationToken).ConfigureAwait(false);
        if (restoreFilesResult.IsFailure)
            return restoreFilesResult.Errors;

        theme.IsDeleted = false;
        theme.IsCurrent = null;
        theme.UpdatedOnUtc = DateTime.UtcNow;
        theme.UpdatedBy = userId;
        Result<Updated> updateResult = await _unitOfWork.ThemeRepository.UpdateAsync(theme, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
            return updateResult.Errors;

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}
