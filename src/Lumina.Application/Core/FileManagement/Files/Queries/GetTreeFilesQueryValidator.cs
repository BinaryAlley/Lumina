#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Files.Queries;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTreeFilesQuery"/>.
/// </summary>
public class GetTreeFilesQueryValidator : AbstractValidator<GetTreeFilesQuery>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryValidator"/> class.
    /// </summary>
    public GetTreeFilesQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }
    #endregion
}