#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Core.Favorites.Queries.GetFavoriteStatus;

/// <summary>
/// Validates the needed validation rules for <see cref="GetFavoriteStatusQuery"/>.
/// </summary>
public class GetFavoriteStatusQueryValidator : AbstractValidator<GetFavoriteStatusQuery>
{
    private const int MAXIMUM_MEDIA_ITEM_COUNT = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFavoriteStatusQueryValidator"/> class.
    /// </summary>
    public GetFavoriteStatusQueryValidator()
    {
        RuleFor(query => query.MediaKind)
            .NotNull()
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid);

        RuleFor(query => query.MediaKind)
            .IsInEnum()
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid)
            .When(query => query.MediaKind is not null);

        RuleFor(query => query.MediaKind)
            .Must(mediaKind => mediaKind != FavoriteMediaKind.None)
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid)
            .When(query => query.MediaKind is not null);

        RuleFor(query => query.MediaItemIds)
            .NotEmpty()
            .WithError(Errors.Favorites.FavoriteMediaItemIdsCannotBeEmpty);

        RuleFor(query => query.MediaItemIds)
            .Must(mediaItemIds => mediaItemIds!.Count <= MAXIMUM_MEDIA_ITEM_COUNT)
            .When(query => query.MediaItemIds is not null)
            .WithError(Errors.Favorites.FavoriteMediaItemIdsExceedMaximumCount);

        RuleFor(query => query.MediaItemIds)
            .Must(mediaItemIds => !mediaItemIds!.Contains(Guid.Empty))
            .When(query => query.MediaItemIds is not null)
            .WithError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }
}
