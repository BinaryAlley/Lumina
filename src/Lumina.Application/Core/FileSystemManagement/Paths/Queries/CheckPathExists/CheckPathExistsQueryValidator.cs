﻿#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

/// <summary>
/// Validates the needed validation rules for <see cref="CheckPathExistsQuery"/>.
/// </summary>
public class CheckPathExistsQueryValidator : AbstractValidator<CheckPathExistsQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckPathExistsQueryValidator"/> class.
    /// </summary>
    public CheckPathExistsQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }
}