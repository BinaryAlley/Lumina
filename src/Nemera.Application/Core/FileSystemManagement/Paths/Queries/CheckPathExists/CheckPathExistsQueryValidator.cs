#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Queries.CheckPathExists;

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
