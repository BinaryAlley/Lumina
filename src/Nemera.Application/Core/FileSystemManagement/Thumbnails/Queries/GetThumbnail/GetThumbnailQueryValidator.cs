#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Infrastructure.Validation;
using Nemera.Application.Common.Utilities;
using ApplicationErrors = Nemera.Application.Common.Errors.Errors;
using DomainErrors = Nemera.Domain.Common.Errors.Errors;
#endregion

namespace Nemera.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Validates the needed validation rules for <see cref="GetThumbnailQuery"/>.
/// </summary>
public class GetThumbnailQueryValidator : AbstractValidator<GetThumbnailQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailQueryValidator"/> class.
    /// </summary>
    public GetThumbnailQueryValidator()
    {
        RuleFor(query => query.Path)
            .NotEmpty()
            .WithError(DomainErrors.FileSystemManagement.PathCannotBeEmpty);
        
        RuleFor(query => query.Quality)
            .InclusiveBetween(0, 100)
            .WithError(ApplicationErrors.Thumbnails.ImageQualityMustBeBetweenZeroAndOneHundred);
    }
}
