#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Application.Core.Themes.Management.Queries.GetThemeTemplate;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.Themes.Management.Queries.GetThemeTemplate;

/// <summary>
/// Validates the needed validation rules for <see cref="GetThemeTemplateQuery"/>.
/// </summary>
public class GetThemeTemplateQueryValidator : AbstractValidator<GetThemeTemplateQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeTemplateQueryValidator"/> class.
    /// </summary>
    public GetThemeTemplateQueryValidator()
    {
        RuleFor(query => query.ThemeId)
            .NotEmpty()
            .WithError(DomainErrors.Themes.ThemeIdCannotBeEmpty);

        RuleFor(query => query.PageKey)
            .NotEmpty()
            .WithError(DomainErrors.Themes.PageKeyCannotBeEmpty);
    }
}
