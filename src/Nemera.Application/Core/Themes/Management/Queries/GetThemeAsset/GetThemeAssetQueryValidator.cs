#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;

/// <summary>
/// Validates the needed validation rules for <see cref="GetThemeAssetQuery"/>.
/// </summary>
public class GetThemeAssetQueryValidator : AbstractValidator<GetThemeAssetQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeAssetQueryValidator"/> class.
    /// </summary>
    public GetThemeAssetQueryValidator()
    {
        RuleFor(query => query.ThemeId)
            .NotEmpty()
            .WithError(DomainErrors.Themes.ThemeIdCannotBeEmpty);

        RuleFor(query => query.AssetPath)
            .NotEmpty()
            .WithError(DomainErrors.Themes.ThemeAssetPathCannotBeEmpty);
    }
}
