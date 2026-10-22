#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Common.Errors;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Core.Favorites.Commands.RemoveFavorite;

/// <summary>
/// Validates the needed validation rules for <see cref="RemoveFavoriteCommand"/>.
/// </summary>
public class RemoveFavoriteCommandValidator : AbstractValidator<RemoveFavoriteCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveFavoriteCommandValidator"/> class.
    /// </summary>
    public RemoveFavoriteCommandValidator()
    {
        RuleFor(command => command.MediaKind)
            .NotEmpty()
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid);

        // Only the member names are accepted, so a numeric route value that happens to parse as an enumeration value is rejected.
        RuleFor(command => command.MediaKind)
            .Must(mediaKind => Enum.GetNames<FavoriteMediaKind>().Contains(mediaKind, StringComparer.OrdinalIgnoreCase))
            .When(command => command.MediaKind is not null && command.MediaKind.Length > 0)
            .WithError(Errors.Favorites.FavoriteMediaKindIsInvalid);

        RuleFor(command => command.MediaItemId)
            .NotEmpty()
            .WithError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);

        RuleFor(command => command.MediaItemId)
            .Must(mediaItemId => Guid.TryParse(mediaItemId, out Guid parsedMediaItemId) && parsedMediaItemId != Guid.Empty)
            .When(command => command.MediaItemId is not null && command.MediaItemId.Length > 0)
            .WithError(Errors.Favorites.FavoriteMediaItemIdCannotBeEmpty);
    }
}
