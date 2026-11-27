#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;

/// <summary>
/// Validates the needed validation rules for <see cref="GetPathParentQuery"/>.
/// </summary>
public class GetPathParentQueryValidator : AbstractValidator<GetPathParentQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPathParentQueryValidator"/> class.
    /// </summary>
    public GetPathParentQueryValidator()
    {
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
