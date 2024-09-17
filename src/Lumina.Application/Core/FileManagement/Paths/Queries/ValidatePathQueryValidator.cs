#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Queries;

/// <summary>
/// Validates the needed validation rules for <see cref="ValidatePathQuery"/>.
/// </summary>
public class ValidatePathQueryValidator : AbstractValidator<ValidatePathQuery>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryValidator"/> class.
    /// </summary>
    public ValidatePathQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }
    #endregion
}