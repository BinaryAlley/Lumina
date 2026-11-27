#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Errors;
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Application.Core.Themes.Management.Commands.InstallTheme;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.Themes.Management.Commands.InstallTheme;

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
