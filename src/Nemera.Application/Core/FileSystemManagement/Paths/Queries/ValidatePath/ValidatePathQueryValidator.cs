#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;

/// <summary>
/// Validates the needed validation rules for <see cref="ValidatePathQuery"/>.
/// </summary>
public class ValidatePathQueryValidator : AbstractValidator<ValidatePathQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatePathQueryValidator"/> class.
    /// </summary>
    public ValidatePathQueryValidator()
    {
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
