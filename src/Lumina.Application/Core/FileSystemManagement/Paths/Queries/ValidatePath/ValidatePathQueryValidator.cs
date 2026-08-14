#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.ValidatePath;

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
