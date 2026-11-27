#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;

/// <summary>
/// Validates the needed validation rules for <see cref="GetDirectoriesQuery"/>.
/// </summary>
public class GetDirectoriesQueryValidator : AbstractValidator<GetDirectoriesQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesQueryValidator"/> class.
    /// </summary>
    public GetDirectoriesQueryValidator()
    {
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
