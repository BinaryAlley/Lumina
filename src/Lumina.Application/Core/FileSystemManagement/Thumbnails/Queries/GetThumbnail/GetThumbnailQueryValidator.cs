#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileSystemManagement.Thumbnails.Queries.GetThumbnail;

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
            .WithError(Errors.FileSystemManagement.PathCannotBeEmpty);
        
        RuleFor(query => query.Quality)
            .InclusiveBetween(0, 100)
            .WithError(Errors.Thumbnails.ImageQualityMustBeBetweenZeroAndOneHundred);
    }
}
