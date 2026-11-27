#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Paths.Queries.GetPathRoot;

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
