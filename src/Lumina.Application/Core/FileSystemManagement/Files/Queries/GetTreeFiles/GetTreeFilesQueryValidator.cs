#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.SharedKernel.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;

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
