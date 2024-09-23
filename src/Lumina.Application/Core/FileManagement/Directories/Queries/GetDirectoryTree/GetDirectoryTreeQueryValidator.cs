#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries.GetDirectoryTree;

/// <summary>
/// Validates the needed validation rules for <see cref="GetDirectoryTreeQuery"/>.
/// </summary>
public class GetDirectoryTreeQueryValidator : AbstractValidator<GetDirectoryTreeQuery>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeQueryValidator"/> class.
    /// </summary>
    public GetDirectoryTreeQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }
    #endregion
}