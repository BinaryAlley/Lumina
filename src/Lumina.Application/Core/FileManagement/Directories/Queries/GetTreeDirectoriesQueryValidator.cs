﻿#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Directories.Queries;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTreeDirectoriesQuery"/>.
/// </summary>
public class GetTreeDirectoriesQueryValidator : AbstractValidator<GetTreeDirectoriesQuery>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesQueryValidator"/> class.
    /// </summary>
    public GetTreeDirectoriesQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }
    #endregion
}