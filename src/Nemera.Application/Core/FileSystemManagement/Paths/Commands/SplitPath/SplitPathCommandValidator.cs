#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Commands.SplitPath;

/// <summary>
/// Validates the needed validation rules for <see cref="SplitPathCommand"/>.
/// </summary>
public class SplitPathCommandValidator : AbstractValidator<SplitPathCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandValidator"/> class.
    /// </summary>
    public SplitPathCommandValidator()
    {
        RuleFor(command => command.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
