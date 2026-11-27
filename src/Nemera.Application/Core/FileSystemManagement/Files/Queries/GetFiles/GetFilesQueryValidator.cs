#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using Nemera.Domain.Common.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Files.Queries.GetFiles;

/// <summary>
/// Validates the needed validation rules for <see cref="GetFilesQuery"/>.
/// </summary>
public class GetFilesQueryValidator : AbstractValidator<GetFilesQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesQueryValidator"/> class.
    /// </summary>
    public GetFilesQueryValidator()
    {
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
    }
}
