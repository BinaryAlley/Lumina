﻿#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Paths.Commands;

/// <summary>
/// Validates the needed validation rules for <see cref="SplitPathCommand"/>.
/// </summary>
public class SplitPathCommandValidator : AbstractValidator<SplitPathCommand>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="SplitPathCommandValidator"/> class.
    /// </summary>
    public SplitPathCommandValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
    }
    #endregion
}