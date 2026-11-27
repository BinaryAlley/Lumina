#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

/// <summary>
/// Validates the needed validation rules for <see cref="GetTreeFilesQuery"/>.
/// </summary>
public class GetTreeFilesQueryValidator : AbstractValidator<GetTreeFilesQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesQueryValidator"/> class.
    /// </summary>
    public GetTreeFilesQueryValidator()
    {
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
