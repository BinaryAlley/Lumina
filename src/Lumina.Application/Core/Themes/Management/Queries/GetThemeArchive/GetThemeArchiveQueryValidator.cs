#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;

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
