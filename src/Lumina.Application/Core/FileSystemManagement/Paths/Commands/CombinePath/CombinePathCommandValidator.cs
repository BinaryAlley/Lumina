#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Commands.CombinePath;

/// <summary>
/// Validates the needed validation rules for <see cref="CombinePathCommand"/>.
/// </summary>
public class CombinePathCommandValidator : AbstractValidator<CombinePathCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathCommandValidator"/> class.
    /// </summary>
    public CombinePathCommandValidator()
    {
        RuleFor(command => command.OriginalPath)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
       
        RuleFor(command => command.NewPath)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
