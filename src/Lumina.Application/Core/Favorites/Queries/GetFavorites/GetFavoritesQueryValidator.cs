#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
#endregion

namespace Lumina.Application.Core.Favorites.Queries.GetFavorites;

/// <summary>
/// Validates the needed validation rules for <see cref="GetFavoritesQuery"/>.
/// </summary>
public class GetFavoritesQueryValidator : AbstractValidator<GetFavoritesQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoritesQueryValidator"/> class.
    /// </summary>
    public GetFavoritesQueryValidator()
    {
        RuleFor(query => query.MediaKind)
            .IsInEnum()
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid)
            .When(query => query.MediaKind is not null);
    }
}
