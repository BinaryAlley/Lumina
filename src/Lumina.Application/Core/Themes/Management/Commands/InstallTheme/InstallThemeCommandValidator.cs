#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Errors;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Application.Core.Themes.Management.Commands.InstallTheme;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Core.Themes.Management.Commands.InstallTheme;

/// <summary>
/// Validates the needed validation rules for <see cref="InstallThemeCommand"/>.
/// </summary>
public class InstallThemeCommandValidator : AbstractValidator<InstallThemeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallThemeCommandValidator"/> class.
    /// </summary>
    public InstallThemeCommandValidator()
    {
        RuleFor(command => command.Archive)
            .NotNull()
            .WithError(DomainErrors.Themes.ThemeArchiveCannotBeNull);
    }
}
