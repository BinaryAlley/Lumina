#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;

/// <summary>
/// Handler for the command to set the currently active theme.
/// </summary>
public class SetCurrentThemeCommandHandler : ICommandHandler<SetCurrentThemeCommand, Result<ThemeResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<SetCurrentThemeCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public SetCurrentThemeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<SetCurrentThemeCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to set the currently active theme.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the newly active <see cref="ThemeResponse"/>, or an error.
    /// </returns>
    public async Task<Result<ThemeResponse>> HandleAsync(SetCurrentThemeCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // only admins can change the active theme
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        Result<ThemeEntity?> getThemeResult = await _unitOfWork.ThemeRepository.GetByThemeIdAsync(command.ThemeId!, cancellationToken).ConfigureAwait(false);
        if (getThemeResult.IsFailure)
            return getThemeResult.Errors;

        ThemeEntity? theme = getThemeResult.Value;
        if (theme is null || theme.IsDeleted)
            return DomainErrors.Themes.ThemeNotFound;

        if (theme.IsCurrent == true)
            return theme.ToResponse();

        Result<ThemeEntity?> getCurrentResult = await _unitOfWork.ThemeRepository.GetCurrentAsync(cancellationToken).ConfigureAwait(false);
        if (getCurrentResult.IsFailure)
            return getCurrentResult.Errors;

        ThemeEntity? currentTheme = getCurrentResult.Value;
        if (currentTheme is not null && currentTheme.Id != theme.Id)
        {
            currentTheme.IsCurrent = null;
            await _unitOfWork.ThemeRepository.UpdateAsync(currentTheme, cancellationToken).ConfigureAwait(false);
        }

        theme.IsCurrent = true;
        theme.UpdatedOnUtc = DateTime.UtcNow;
        theme.UpdatedBy = userId;
        await _unitOfWork.ThemeRepository.UpdateAsync(theme, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return theme.ToResponse();
    }
}
