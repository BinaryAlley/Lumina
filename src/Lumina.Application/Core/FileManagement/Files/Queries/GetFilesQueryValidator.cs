#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Files.Queries;

/// <summary>
/// Validates the needed validation rules for <see cref="GetFilesQuery"/>.
/// </summary>
public class GetFilesQueryValidator : AbstractValidator<GetFilesQuery>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesQueryValidator"/> class.
    /// </summary>
    public GetFilesQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }
    #endregion
}