#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;

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
