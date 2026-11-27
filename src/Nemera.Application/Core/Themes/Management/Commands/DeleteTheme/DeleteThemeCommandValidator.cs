#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Application.Core.Themes.Management.Commands.DeleteTheme;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.Themes.Management.Commands.DeleteTheme;

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
