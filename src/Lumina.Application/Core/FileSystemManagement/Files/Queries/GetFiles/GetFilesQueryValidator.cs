#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;

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
