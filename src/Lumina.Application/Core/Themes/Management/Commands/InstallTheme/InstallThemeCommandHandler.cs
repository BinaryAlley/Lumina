#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Repositories.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Authentication;
using Lumina.Application.Common.Infrastructure.Authorization;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApplicationErrors = Lumina.Application.Common.Errors.Errors;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.InstallTheme;

/// <summary>
/// Handler for the command to install a theme pack.
/// </summary>
public class InstallThemeCommandHandler : ICommandHandler<InstallThemeCommand, Result<ThemeResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IThemeService _themeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IValidator<InstallThemeCommand> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="currentUserService">Injected service to retrieve the current user information.</param>
    /// <param name="authorizationService">Injected service for authorization related functionality.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public InstallThemeCommandHandler(
        IUnitOfWork unitOfWork,
        IThemeService themeService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IValidator<InstallThemeCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _themeService = themeService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the command to install a theme pack.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully installed <see cref="ThemeResponse"/>, or an error.
    /// </returns>
    public async Task<Result<ThemeResponse>> HandleAsync(InstallThemeCommand command, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(command);
        if (validationResult.Count > 0)
            return validationResult;

        Guid? currentUserId = _currentUserService.UserId;
        if (currentUserId is null)
            return ApplicationErrors.Authorization.NotAuthorized;
        Guid userId = currentUserId.Value;

        // only admins can install themes
        if (!await _authorizationService.IsInRoleAsync(userId, "Admin", cancellationToken).ConfigureAwait(false))
            return ApplicationErrors.Authorization.NotAuthorized;

        // store the theme pack files on the server, replacing the files of an existing theme with the same manifest id
        Result<ThemeManifestDto> installResult = await _themeService.InstallAsync(command.Archive!, cancellationToken).ConfigureAwait(false);
        if (installResult.IsFailure)
            return installResult.Errors;

        ThemeManifestDto manifest = installResult.Value;
        Result<ThemeEntity?> getThemeResult = await _unitOfWork.ThemeRepository.GetByThemeIdAsync(manifest.Id, cancellationToken).ConfigureAwait(false);
        if (getThemeResult.IsFailure)
            return getThemeResult.Errors;

        ThemeEntity themeEntity;
        if (getThemeResult.Value is not null)
        {
            ThemeEntity existingTheme = getThemeResult.Value;
            themeEntity = new ThemeEntity
            {
                Id = existingTheme.Id,
                ThemeId = manifest.Id,
                Name = manifest.Name,
                Description = manifest.Description,
                Author = manifest.Author,
                Version = manifest.Version,
                PreviewPath = manifest.Preview,
                InstallSource = existingTheme.InstallSource,
                IsCurrent = existingTheme.IsCurrent,
                IsDeleted = existingTheme.IsDeleted,
                InstalledAtUtc = DateTime.UtcNow,
                CreatedOnUtc = existingTheme.CreatedOnUtc,
                CreatedBy = existingTheme.CreatedBy,
                UpdatedOnUtc = DateTime.UtcNow,
                UpdatedBy = userId
            };

            Result<Updated> updateResult = await _unitOfWork.ThemeRepository.UpdateAsync(themeEntity, cancellationToken).ConfigureAwait(false);
            if (updateResult.IsFailure)
                return updateResult.Errors;
        }
        else
        {
            themeEntity = new ThemeEntity
            {
                Id = Guid.NewGuid(),
                ThemeId = manifest.Id,
                Name = manifest.Name,
                Description = manifest.Description,
                Author = manifest.Author,
                Version = manifest.Version,
                PreviewPath = manifest.Preview,
                InstallSource = ThemeInstallSource.Uploaded,
                InstalledAtUtc = DateTime.UtcNow,
                CreatedOnUtc = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            Result<Created> insertResult = await _unitOfWork.ThemeRepository.InsertAsync(themeEntity, cancellationToken).ConfigureAwait(false);
            if (insertResult.IsFailure)
            {
                // roll back the stored files so a failed install leaves no orphaned theme pack behind
                await _themeService.DeleteAsync(manifest.Id, cancellationToken).ConfigureAwait(false);
                return insertResult.Errors;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return themeEntity.ToResponse();
    }
}
