﻿#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTreeFilesQuery"/>.
/// </summary>
public class GetTreeFilesQueryValidator : AbstractValidator<GetTreeFilesQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryValidator"/> class.
    /// </summary>
    public GetTreeFilesQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }
}