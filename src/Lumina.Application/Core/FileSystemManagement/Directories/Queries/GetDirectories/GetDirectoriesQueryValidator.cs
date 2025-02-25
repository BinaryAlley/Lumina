﻿#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Validates the needed validation rules for <see cref="GetDirectoriesQuery"/>.
/// </summary>
public class GetDirectoriesQueryValidator : AbstractValidator<GetDirectoriesQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryValidator"/> class.
    /// </summary>
    public GetDirectoriesQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }
}