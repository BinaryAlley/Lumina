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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;

/// <summary>
/// Handler for the command to delete a theme.
/// </summary>
public class DeleteThemeCommandHandler : ICommandHandler<DeleteThemeCommand, Result<Success>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IThemeService _themeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<DeleteThemeCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public DeleteThemeCommandHandler(
        IUnitOfWork unitOfWork,
        IThemeService themeService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<DeleteThemeCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _themeService = themeService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to delete a theme.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> representing either a successful operation, or an error.
    /// </returns>
    public async Task<Result<Success>> HandleAsync(DeleteThemeCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // only admins can delete themes
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<ThemeEntity?> getThemeResult = await _unitOfWork.ThemeRepository.GetByThemeIdAsync(command.ThemeId!, cancellationToken).ConfigureAwait(false);
        if (getThemeResult.IsFailure)
            return getThemeResult.Errors;

        ThemeEntity? theme = getThemeResult.Value;
        if (theme is null || theme.IsDeleted)
            return DomainErrors.Themes.ThemeNotFound;

        Result<IEnumerable<ThemeEntity>> getAllResult = await _unitOfWork.ThemeRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (getAllResult.IsFailure)
            return getAllResult.Errors;

        List<ThemeEntity> availableThemes = getAllResult.Value.Where(availableTheme => !availableTheme.IsDeleted).ToList();

        // at least one bundled theme must always remain available, so the application never ends up without any theme
        if (theme.InstallSource == ThemeInstallSource.Bundled)
        {
            int availableBundledThemes = availableThemes.Count(availableTheme => availableTheme.InstallSource == ThemeInstallSource.Bundled);
            if (availableBundledThemes <= 1)
                return DomainErrors.Themes.LastBundledThemeCannotBeDeleted;
        }

        // if the deleted theme was the active one, switch to another available theme, preferring the configured default
        if (theme.IsCurrent == true)
        {
            ThemeEntity? replacementTheme = availableThemes
                .Where(availableTheme => availableTheme.Id != theme.Id)
                .OrderBy(availableTheme => string.Equals(availableTheme.ThemeId, _themeService.DefaultThemeId, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(availableTheme => availableTheme.ThemeId, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (replacementTheme is null)
                return DomainErrors.Themes.ThemeCannotBeDeleted;

            replacementTheme.IsCurrent = true;
            await _unitOfWork.ThemeRepository.UpdateAsync(replacementTheme, cancellationToken).ConfigureAwait(false);
        }

        if (theme.InstallSource == ThemeInstallSource.Bundled)
        {
            // bundled themes are soft deleted, so they can be restored automatically later, as long as the user did not delete them
            theme.IsDeleted = true;
            theme.IsCurrent = null;
            theme.UpdatedOnUtc = DateTime.UtcNow;
            theme.UpdatedBy = userId;
            await _unitOfWork.ThemeRepository.UpdateAsync(theme, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // user themes are removed entirely, because their files are not shipped with the application and could never be restored
            Result<Deleted> deleteResult = await _unitOfWork.ThemeRepository.DeleteByIdAsync(theme.Id, cancellationToken).ConfigureAwait(false);
            if (deleteResult.IsFailure)
                return deleteResult.Errors;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Result<Success> deleteFilesResult = await _themeService.DeleteAsync(theme.ThemeId, cancellationToken).ConfigureAwait(false);
        if (deleteFilesResult.IsFailure)
            return deleteFilesResult.Errors;

        return Result.Success;
    }
}
