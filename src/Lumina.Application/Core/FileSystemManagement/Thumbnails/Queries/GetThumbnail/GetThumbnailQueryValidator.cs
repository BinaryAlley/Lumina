#region ========================================================================= USING =====================================================================================
using FluentValidation;
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
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileSystemManagement.PathCannotBeEmpty.Description);
        RuleFor(x => x.Quality).InclusiveBetween(0, 100).WithMessage(Errors.Thumbnails.ImageQualityMustBeBetweenZeroAndOneHundred.Description);
    }
}
