#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;

/// <summary>
/// Validates the needed validation rules for <see cref="DeleteThemeCommand"/>.
/// </summary>
public class DeleteThemeCommandValidator : AbstractValidator<DeleteThemeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteThemeCommandValidator"/> class.
    /// </summary>
    public DeleteThemeCommandValidator()
    {
        RuleFor(command => command.ThemeId)
            .NotEmpty()
            .WithError(DomainErrors.Themes.ThemeIdCannotBeEmpty);
    }
}
