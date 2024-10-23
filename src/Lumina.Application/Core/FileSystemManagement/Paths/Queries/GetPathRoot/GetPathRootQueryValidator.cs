#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

/// <summary>
/// Validates the needed validation rules for <see cref="GetPathRootQuery"/>.
/// </summary>
public class GetPathRootQueryValidator : AbstractValidator<GetPathRootQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathRootQueryValidator"/> class.
    /// </summary>
    public GetPathRootQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
    }
}