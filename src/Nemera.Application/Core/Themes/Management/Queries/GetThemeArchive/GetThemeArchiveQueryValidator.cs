#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Application.Core.Themes.Management.Queries.GetThemeArchive;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.Themes.Management.Queries.GetThemeArchive;

/// <summary>
/// Validates the needed validation rules for <see cref="GetThemeArchiveQuery"/>.
/// </summary>
public class GetThemeArchiveQueryValidator : AbstractValidator<GetThemeArchiveQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThemeArchiveQueryValidator"/> class.
    /// </summary>
    public GetThemeArchiveQueryValidator()
    {
        RuleFor(query => query.ThemeId)
            .NotEmpty()
            .WithError(DomainErrors.Themes.ThemeIdCannotBeEmpty);
    }
}
