#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Paths.Queries.GetPathParent;

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
