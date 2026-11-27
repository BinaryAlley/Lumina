#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.Themes.Management.Commands.SetCurrentTheme;

/// <summary>
/// Validates the needed validation rules for <see cref="SetCurrentThemeCommand"/>.
/// </summary>
public class SetCurrentThemeCommandValidator : AbstractValidator<SetCurrentThemeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetCurrentThemeCommandValidator"/> class.
    /// </summary>
    public SetCurrentThemeCommandValidator()
    {
        RuleFor(command => command.ThemeId)
            .NotEmpty()
            .WithError(DomainErrors.Themes.ThemeIdCannotBeEmpty);
    }
}
