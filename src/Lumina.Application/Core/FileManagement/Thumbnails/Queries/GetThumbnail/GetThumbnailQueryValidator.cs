#region ========================================================================= USING =====================================================================================
using FluentValidation;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.FileManagement.Thumbnails.Queries.GetThumbnail;

/// <summary>
/// Validates the needed validation rules for <see cref="GetThumbnailQuery"/>.
/// </summary>
public class GetThumbnailQueryValidator : AbstractValidator<GetThumbnailQuery>
{
    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="GetThumbnailQueryValidator"/> class.
    /// </summary>
    public GetThumbnailQueryValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(Errors.FileManagement.PathCannotBeEmpty.Code);
        RuleFor(x => x.Quality).InclusiveBetween(0, 100).WithMessage(Errors.Thumbnails.ImageQaulityMustBeBetweenZeroAndOneHundred.Code);
    }
    #endregion
}