#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;

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
