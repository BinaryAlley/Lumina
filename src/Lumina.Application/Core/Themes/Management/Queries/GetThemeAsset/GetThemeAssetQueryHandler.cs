#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.UoW;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using Lumina.Application.Common.Infrastructure.Themes;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Contracts.Responses.Themes;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Themes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;

/// <summary>
/// Handler for the query to get an asset file of a theme.
/// </summary>
public class GetThemeAssetQueryHandler : IQueryHandler<GetThemeAssetQuery, Result<ThemeAssetResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IThemeService _themeService;
    private readonly IValidator<GetThemeAssetQuery> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeAssetQueryHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">Injected unit of work for interacting with the data access layer repositories.</param>
    /// <param name="themeService">Injected service for the server-side storage and serving of theme packs.</param>
    /// <param name="validator">Injected validator for application validation rules.</param>
    public GetThemeAssetQueryHandler(IUnitOfWork unitOfWork, IThemeService themeService, IValidator<GetThemeAssetQuery> validator)
    {
        _unitOfWork = unitOfWork;
        _themeService = themeService;
        _validator = validator;
    }

    /// <summary>
    /// Handles the query to get an asset file of a theme.
    /// </summary>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either the asset of the theme, or an error.
    /// </returns>
    public async Task<Result<ThemeAssetResponse>> HandleAsync(GetThemeAssetQuery query, CancellationToken cancellationToken)
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

        Result<ThemeAssetDto> assetResult = await _themeService.GetAssetAsync(theme.ThemeId, query.AssetPath!, cancellationToken).ConfigureAwait(false);
        if (assetResult.IsFailure)
        {
            // the stored files of a bundled theme removed externally are restored, so its assets keep being served
            if (theme.InstallSource == ThemeInstallSource.Bundled)
            {
                Result<Success> restoreResult = await _themeService.RestoreBundledThemeAsync(theme.ThemeId, cancellationToken).ConfigureAwait(false);
                if (restoreResult.IsFailure)
                    return restoreResult.Errors;

                assetResult = await _themeService.GetAssetAsync(theme.ThemeId, query.AssetPath!, cancellationToken).ConfigureAwait(false);
            }

            if (assetResult.IsFailure)
                return assetResult.Errors;
        }

        return new ThemeAssetResponse(assetResult.Value.Bytes, assetResult.Value.ContentType);
    }
}
