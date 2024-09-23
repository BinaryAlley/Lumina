#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Commands.CombinePath;

/// <summary>
/// Validates the needed validation rules for <see cref="CombinePathCommand"/>.
/// </summary>
public class CombinePathCommandValidator : AbstractValidator<CombinePathCommand>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="CombinePathCommandValidator"/> class.
    /// </summary>
    public CombinePathCommandValidator()
    {
        RuleFor(x => x.OriginalPath).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
        RuleFor(x => x.NewPath).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }
    #endregion
}