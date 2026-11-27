#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.Plugins.Commands.InstallPlugin;

/// <summary>
/// Validates the needed validation rules for <see cref="InstallPluginCommand"/>.
/// </summary>
public class InstallPluginCommandValidator : AbstractValidator<InstallPluginCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstallPluginCommandValidator"/> class.
    /// </summary>
    public InstallPluginCommandValidator()
    {
        RuleFor(command => command.Archive)
            .NotNull()
            .WithError(Errors.Plugins.PluginArchiveCannotBeNull);

        RuleFor(command => command.FileName)
            .NotEmpty()
            .WithError(Errors.Plugins.PluginFileNameCannotBeEmpty);
    }
}
