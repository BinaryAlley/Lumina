#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.Plugins.Commands.InstallPlugin;

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
