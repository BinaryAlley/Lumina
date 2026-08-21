#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;

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
