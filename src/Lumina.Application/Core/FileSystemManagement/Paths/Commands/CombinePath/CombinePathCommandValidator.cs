#region ========================================================================= USING =====================================================================================
using FluentValidation;
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
        RuleFor(x => x.OriginalPath).NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
        RuleFor(x => x.NewPath).NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }
}
