#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Mapping.Themes;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeTemplate;

/// <summary>
/// Handler for the query to get the template of a theme.
/// </summary>
public class GetThemeTemplateQueryHandler : IQueryHandler<GetThemeTemplateQuery, Result<ThemeTemplateResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IThemeService _themeService;
    private readonly IValidator<GetThemeTemplateQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeTemplateQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetThemeTemplateQueryHandler(IUnitOfWork unitOfWork, IThemeService themeService, IValidator<GetThemeTemplateQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _themeService = themeService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get the template of a theme.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the template of the theme, or an error.
    /// </returns>
    public async Task<Result<ThemeTemplateResponse>> HandleAsync(GetThemeTemplateQuery query, CancellationToken cancellationToken)
    {
        List<Error> validationResult = _validator.Validate(query);
        if (validationResult.Count > 0)
            return validationResult;

        Result<ThemeEntity?> getThemeResult = await _unitOfWork.ThemeRepository.GetByThemeIdAsync(query.ThemeId!, cancellationToken).ConfigureAwait(false);
        if (getThemeResult.IsFailure)
            return getThemeResult.Errors;

        ThemeEntity? theme = getThemeResult.Value;
        if (theme is null || theme.IsDeleted)
            return DomainErrors.Themes.ThemeNotFound;

        Result<string> templateResult = await _themeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, cancellationToken).ConfigureAwait(false);
        if (templateResult.IsFailure)
        {
            // the stored files of a bundled theme removed externally are restored, and if the broken theme was the
            // active one, the configured default theme is activated, so clients always have a renderable theme
            if (theme.InstallSource == ThemeInstallSource.Bundled && await TryRestoreBundledThemeAsync(theme, cancellationToken).ConfigureAwait(false))
                templateResult = await _themeService.GetTemplateAsync(theme.ThemeId, query.PageKey!, cancellationToken).ConfigureAwait(false);

            if (templateResult.IsFailure)
                return templateResult.Errors;
        }

        return new ThemeTemplateResponse(theme.ToResponse(), templateResult.Value);
    }

    /// <summary>
    /// Restores the files of a bundled theme whose stored files were removed externally, and switches the active theme to the configured default when the broken theme was the active one.
    /// </summary>
    /// <param name="theme">The bundled theme to restore.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns><see langword="true"/> when the theme was restored; <see langword="false"/> otherwise.</returns>
    private async Task<bool> TryRestoreBundledThemeAsync(ThemeEntity theme, CancellationToken cancellationToken)
    {
        Result<Success> restoreResult = await _themeService.RestoreBundledThemeAsync(theme.ThemeId, cancellationToken).ConfigureAwait(false);
        if (restoreResult.IsFailure)
            return false;

        if (theme.IsCurrent == true)
        {
            Result<ThemeEntity?> getDefaultResult = await _unitOfWork.ThemeRepository.GetByThemeIdAsync(_themeService.DefaultThemeId, cancellationToken).ConfigureAwait(false);
            if (getDefaultResult.IsFailure)
                return true;

            ThemeEntity? defaultTheme = getDefaultResult.Value;
            if (defaultTheme is not null && !defaultTheme.IsDeleted && defaultTheme.Id != theme.Id)
            {
                theme.IsCurrent = null;
                await _unitOfWork.ThemeRepository.UpdateAsync(theme, cancellationToken).ConfigureAwait(false);

                defaultTheme.IsCurrent = true;
                await _unitOfWork.ThemeRepository.UpdateAsync(defaultTheme, cancellationToken).ConfigureAwait(false);

                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        return true;
    }
}
