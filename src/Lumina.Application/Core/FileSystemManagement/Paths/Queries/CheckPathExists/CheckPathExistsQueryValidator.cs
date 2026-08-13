#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
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
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
