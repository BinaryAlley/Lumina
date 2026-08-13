#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
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
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
