#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Application.Core.Themes.Management.Commands.RestoreTheme;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.Themes.Management.Commands.RestoreTheme;

/// <summary>
/// Validates the needed validation rules for <see cref="RestoreThemeCommand"/>.
/// </summary>
public class RestoreThemeCommandValidator : AbstractValidator<RestoreThemeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreThemeCommandValidator"/> class.
    /// </summary>
    public RestoreThemeCommandValidator()
    {
        RuleFor(command => command.ThemeId)
            .NotEmpty()
            .WithError(DomainErrors.Themes.ThemeIdCannotBeEmpty);
    }
}
